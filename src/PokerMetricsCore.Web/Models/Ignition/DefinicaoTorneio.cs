using System.ComponentModel.DataAnnotations.Schema;

namespace PokerMetricsCore.Web.Models
{
    // Tabela preenchida pelo data seeding do contexto que conecta um valor de buy-in ao nome de um torneio
    public class DefinicaoTorneio
    {
        public int DefinicaoTorneioId { get; set; }
        public string Nome { get; set; } = null!; // "$2000 LOW ROLLER"
        public decimal BuyIn { get; set; } // Ex: 16.5
        public TimeOnly HorarioComeco { get; set; } // Horário de início do torneio
    }
}