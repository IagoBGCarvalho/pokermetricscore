using System;
using System.Collections.Generic;

namespace PokerMetricsCore.Web.Models
{
    // Modelo que representa os arquivos.xlsx que foram processados
    public class Arquivo
    {
        public int ArquivoId { get; set; }
        public string NomeOriginalArquivo { get; set; } = null!;
        public DateTime DataUploadUtc { get; set; }
        public DateTime PeriodStartDate { get; set; } // Do cabeçalho do arquivo [1]
        public DateTime PeriodEndDate { get; set; } // Do cabeçalho do arquivo [1]
        public int PlayerId { get; set; }
        public virtual Player Player { get; set; } = null!; // PN: 1 arquivo.xlsx pertence a 1 player
        public virtual ICollection<Transacao> Transacoes { get; set; } = new List<Transacao>(); // PN: 1 arquivo.xlsx possui N linhas de registro
    }
}