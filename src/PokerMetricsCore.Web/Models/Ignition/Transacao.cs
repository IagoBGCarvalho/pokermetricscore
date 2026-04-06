using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace PokerMetricsCore.Web.Models
{
    // Tabela que representa uma linha individual do arquivo.xlsx [1]
    public class Transacao
    {
        public long TransacaoId { get; set; }
        public DateTime Data { get; set; }
        public string Descricao { get; set; } = null!; // "Poker Multi Table Tournament Buy-In"
        public string ReferenceId { get; set; } = null!; //"T649207919"
        public decimal ValorMonetario { get; set; } // Ex: -16.5 ou 8.71
        public decimal Pontos { get; set; }
        public int ArquivoId { get; set; }
        public required virtual Arquivo Arquivo { get; set; }
    }
}