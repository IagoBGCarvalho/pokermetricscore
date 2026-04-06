using Microsoft.EntityFrameworkCore;
using PokerMetricsCore.Web.Data;
using PokerMetricsCore.Web.Models;

namespace PokerMetricsCore.Web.Services;

public class RelatorioService
{
    // Serviço responsável por gerar o relatório a partir dos dados extraídos do processamento do .xlsx.
    private readonly IDbContextFactory<PokerMetricsCoreContext> _contextFactory;

    public RelatorioService(IDbContextFactory<PokerMetricsCoreContext> contextFactory)
    {
        _contextFactory = contextFactory;
    }

    public async Task<List<PerformanceTorneioDto>> GetPerformanceSummaryAsync()
    {
        // Consulta LINQ para agrupar por nome de torneio, resumir os resultados e gerar a view
        using var context = await _contextFactory.CreateDbContextAsync();

        return await context.TorneioJogado
          .Include(pt => pt.DefinicaoTorneio) // Inclui os dados do Torneio
          .GroupBy(pt => pt.DefinicaoTorneio.Nome)
          .Select(group => new PerformanceTorneioDto
            {
                NomeTorneio = group.Key,
                TotalEntradas = group.Count(),
                TotalNetResult = group.Sum(pt => pt.NetResult),
                TotalBuyIn = group.Sum(pt => pt.TotalBuyIn),
                // Calcula o Retorno Sobre Investimento (ROI)
                ROI = (group.Sum(pt => pt.TotalBuyIn) == 0)? 0 : (group.Sum(pt => pt.NetResult) / group.Sum(pt => pt.TotalBuyIn)) * 100
            })
          .OrderByDescending(dto => dto.ROI)  // Ordena a partir dos torneios com melhor retorno sobre investimento
          .ToListAsync();
    }
}