using ClosedXML.Excel;
using Microsoft.EntityFrameworkCore;
using PokerMetricsCore.Web.Data;
using PokerMetricsCore.Web.Models;
using System.Globalization;

namespace PokerMetricsCore.Web.Services;

// Serviço responsável por fazer a leitura do arquivo .xlsx
public class ProcessamentoArquivoService
{
    private readonly IDbContextFactory<PokerMetricsCoreContext> _contextFactory;
    private readonly IMatchingTorneiosService _matchingService;

    public ProcessamentoArquivoService(IDbContextFactory<PokerMetricsCoreContext> contextFactory, IMatchingTorneiosService matchingService)
    {
        _contextFactory = contextFactory;
        _matchingService = matchingService;
    }

    public async Task ProcessStatementStreamAsync(Stream fileStream, string fileName, string playerName)
    {
        // Cria um contexto novo e descartável para esta operação
        using var context = await _contextFactory.CreateDbContextAsync();

        // Verificação de duplicidade pelo nome do arquivo
        if (await context.Arquivo.AnyAsync(f => f.NomeOriginalArquivo == fileName))
        {
            return;
        }

        // Carrega TODAS as definições de torneio
        var allDefinitions = await context.DefinicaoTorneio.ToListAsync();

        // Busca ou cria o jogador
        var player = await context.Player.FirstOrDefaultAsync(p => p.Name == playerName);
        if (player == null)
        {
            player = new Player { Name = playerName };
            context.Player.Add(player);
            await context.SaveChangesAsync(); // Salva para gerar o ID do novo player
        }

        // Mapeia os dados referentes ao arquivo para adicionar no banco
        var arquivo = new Arquivo
        {
            NomeOriginalArquivo = fileName,
            DataUploadUtc = DateTime.UtcNow,
            PlayerId = player.PlayerId,
            PeriodStartDate = DateTime.UtcNow,
            PeriodEndDate = DateTime.UtcNow
        };
        context.Arquivo.Add(arquivo);

        // Usa o ClosedXML para ler o Stream do .xlsx
        using var workbook = new XLWorkbook(fileStream);
        var worksheet = workbook.Worksheets.First();

        var usedRange = worksheet.RangeUsed();

        if (usedRange == null)
        {
            throw new Exception("O arquivo enviado está completamente vazio."); // Planilha vazia
        }
        
        // Variável para pular todas as linhas até achar o cabeçalho "Date" e puder começar a ler e armazenar no banco
        var rows = usedRange.RowsUsed();
        
        var dataRows = rows
            .SkipWhile(row => row.Cell(1).GetValue<string>() != "Date")
            .Skip(1); // Pula a própria linha que contém "Date"

        // Se não sobrar nada, o arquivo está com formato errado ou vazio
        if (!dataRows.Any())
        {
            throw new Exception("Formato do arquivo inválido: Cabeçalho 'Date' não encontrado.");
        }

        var transacoes = new List<Transacao>();

        foreach (var row in dataRows)
        {
            // Se a célula de data estiver vazia, para, pois é o fim do arquivo)
            if (row.Cell(1).IsEmpty()) continue;

            var rawDate = row.Cell(1).GetDateTime();
            var dataTransacaoBrasilia = rawDate.AddHours(-3); // Conversão de UTC (horário da bodog) para horário de brasília, permitindo utilizar os horários de late register

            string descricao = row.Cell(2).GetValue<string>();
            string referenceId = row.Cell(3).GetValue<string>();

            if (!row.Cell(4).TryGetValue(out decimal valorMonetario)) valorMonetario = 0;
            if (!row.Cell(6).TryGetValue(out decimal pontos)) pontos = 0;

            if (!descricao.Contains("Poker Multi Table Tournament"))
                continue;

            // Mapeia os dados do registro (linha) do arquivo para adicionar no banco
            transacoes.Add(new Transacao
            {
                Data = dataTransacaoBrasilia,
                Descricao = descricao,
                ReferenceId = referenceId,
                ValorMonetario = valorMonetario,
                Pontos = pontos,
                Arquivo = arquivo
            });
        }

        // Agrupamento de transactions por arquivo 
        var groupedTransactions = transacoes
            .GroupBy(t => t.ReferenceId)
            .ToList();

        var torneiosJogados = new List<TorneioJogado>();

        // Lógica de Matching
        foreach (var group in groupedTransactions)
        {
            // Pega as transações de Buy-In (valor negativo)
            var buyIns = group.Where(t => t.ValorMonetario < 0).ToList();
            var payouts = group.Where(t => t.ValorMonetario > 0).ToList();

            // Se não tem buy-in (ex: ticket ou erro), continua, mas trata depois!!
            if (!buyIns.Any()) continue;

            // Pega o valor do primeiro buy-in para identificar o tipo
            decimal firstBuyInAmount = Math.Abs(buyIns.First().ValorMonetario);

            // Pega o horário do PRIMEIRO buy-in
            var firstBuyInDate = group.Min(t => t.Data);
            var buyInTime = TimeOnly.FromDateTime(firstBuyInDate);

            // Filtragem Inicial de torneios por Valor
            var candidates = allDefinitions
                .Where(d => d.BuyIn == firstBuyInAmount)
                .ToList();

            DefinicaoTorneio definition;

            if (candidates.Count == 0)
            {
                // Caso a transação não se encaixe em nenhum torneio no filtro por valor, é retornada como torneio não mapeado
                definition = new DefinicaoTorneio
                {
                    Nome = $"NÃO MAPEADO (${firstBuyInAmount})",
                    BuyIn = firstBuyInAmount,
                    HorarioComeco = buyInTime
                };
            }
            else if (candidates.Count == 1)
            {
                definition = candidates.First();
            }
            else
            {
                definition = _matchingService.AcharTorneioMaisProximo(buyInTime, candidates);
            }

            decimal totalBuyIn = Math.Abs(buyIns.Sum(t => t.ValorMonetario));
            decimal totalPayout = payouts.Sum(t => t.ValorMonetario);

            // Por fim, mapeia uma instância de um torneio jogado e armazena ele no banco
            torneiosJogados.Add(new TorneioJogado
            {
                ReferenceId = group.Key,
                DataComeco = firstBuyInDate,
                TotalBuyIn = totalBuyIn,
                TotalPayout = totalPayout,
                NetResult = totalPayout - totalBuyIn,
                PlayerId = player.PlayerId,
                DefinicaoTorneio = definition
            });
        }

        // Adiciona tudo ao contexto e salva de uma vez
        context.Transacao.AddRange(transacoes);
        context.TorneioJogado.AddRange(torneiosJogados);

        await context.SaveChangesAsync();
    }
}