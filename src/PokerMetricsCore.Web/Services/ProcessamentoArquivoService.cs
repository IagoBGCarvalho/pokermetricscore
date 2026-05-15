using ClosedXML.Excel;
using Microsoft.EntityFrameworkCore;
using PokerMetricsCore.Web.Data;
using PokerMetricsCore.Web.Models;
using System.Globalization;

namespace PokerMetricsCore.Web.Services;

/// <summary>
/// Serviço responsável pelo processamento de arquivos de extrato e orquestração da lógica de negócio.
/// </summary>
/// <remarks>
/// Este serviço lida com a leitura física do arquivo Excel, a conversão de fusos horários,
/// o agrupamento de transações e a persistência final no banco de dados.
/// </remarks>
public class ProcessamentoArquivoService
{
    private readonly IDbContextFactory<PokerMetricsCoreContext> _contextFactory;
    private readonly IMatchingTorneiosService _matchingService;

    /// <summary>
    /// Inicializa uma nova instância da classe <see cref="ProcessamentoArquivoService"/>.
    /// </summary>
    /// <param name="contextFactory">Fábrica para criação do contexto do banco de dados.</param>
    /// <param name="matchingService">Serviço especializado na lógica circular de matching de torneios.</param>
    public ProcessamentoArquivoService(IDbContextFactory<PokerMetricsCoreContext> contextFactory, IMatchingTorneiosService matchingService)
    {
        _contextFactory = contextFactory;
        _matchingService = matchingService;
    }

    /// <summary>
    /// Processa o fluxo de um arquivo .xlsx de extrato da Ignition e salva os resultados.
    /// </summary>
    /// <remarks>
    /// O método realiza a leitura via Stream, converte as datas de UTC para o fuso de Brasília,
    /// filtra transações de MTT e utiliza o <see cref="IMatchingTorneiosService"/> para consolidar os torneios jogados.
    /// </remarks>
    /// <param name="fileStream">O stream de dados do arquivo Excel.</param>
    /// <param name="fileName">O nome original do arquivo para fins de deduplicação no banco.</param>
    /// <param name="playerName">O nome do jogador associado às transações.</param>
    /// <returns>Uma <see cref="Task"/> que representa a operação assíncrona.</returns>
    /// <exception cref="Exception">Lançada se o arquivo estiver vazio ou com o cabeçalho 'Date' ausente.</exception>
    public async Task ProcessamentoFluxoArquivoAsync(Stream fileStream, string fileName, string playerName)
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
            throw new Exception("O arquivo enviado está completamente vazio.");
        }

        // Variável para pular todas as linhas até achar o cabeçalho "Date" e puder começar a ler e armazenar no banco
        var rows = usedRange.RowsUsed();
        var dataRows = rows
            .SkipWhile(row => row.Cell(1).GetValue<string>() != "Date")
            .Skip(1);

        // Se não sobrar nada, o arquivo está com formato errado ou vazio
        if (!dataRows.Any())
        {
            throw new Exception("Formato do arquivo inválido: Cabeçalho 'Date' não encontrado.");
        }

        var transacoes = new List<Transacao>();

        // Definição do Fuso Horário de Brasília
        var brasiliaTimeZone = TimeZoneInfo.FindSystemTimeZoneById("E. South America Standard Time");

        foreach (var row in dataRows)
        {
            // Se a célula de data estiver vazia, para, pois é o fim do arquivo)
            if (row.Cell(1).IsEmpty()) continue;

            // Extração da data original
            var rawDate = row.Cell(1).GetDateTime();
            
            // Delega ao .NET o tratamento da data para UTC
            var dataUtc = DateTime.SpecifyKind(rawDate, DateTimeKind.Utc);
            
            // Conversão de UTC para horário de Brasília ("E. South America Standard Time")
            var dataTransacaoBrasilia = TimeZoneInfo.ConvertTimeFromUtc(dataUtc, brasiliaTimeZone);

            string descricao = row.Cell(2).GetValue<string>();
            string referenceId = row.Cell(3).GetValue<string>();

            if (!row.Cell(4).TryGetValue(out decimal valorMonetario)) valorMonetario = 0;
            if (!row.Cell(6).TryGetValue(out decimal pontos)) pontos = 0;

            if (!descricao.Contains("Poker Multi Table Tournament"))
                continue;
            
            // Mapeia os dados das transações (linhas) do arquivo para adicionar no banco
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

        foreach (var group in groupedTransactions)
        {
            // Pega as transações de Buy-In (valor negativo) e Pay-Out (valor positivo)
            var buyIns = group.Where(t => t.ValorMonetario < 0).ToList();
            var payouts = group.Where(t => t.ValorMonetario > 0).ToList();

            if (!buyIns.Any()) continue;

            // Pega o valor e o horário do primeiro buy-in para identificar o tipo
            decimal firstBuyInAmount = Math.Abs(buyIns.First().ValorMonetario);
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

        context.Transacao.AddRange(transacoes);
        context.TorneioJogado.AddRange(torneiosJogados);

        await context.SaveChangesAsync();
    }
}