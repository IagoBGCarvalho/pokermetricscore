namespace PokerMetricsCore.Web.Models
{
    // DTO que serve para passar os dados para a View do relatório
    public class PerformanceTorneioDto
    {
        public string NomeTorneio { get; set; } = null!;
        public int TotalEntradas { get; set; }
        public decimal TotalNetResult { get; set; }
        public decimal TotalBuyIn { get; set; }
        public decimal ROI { get; set; }
    }
}