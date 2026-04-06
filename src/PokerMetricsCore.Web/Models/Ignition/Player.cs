using System.Collections.Generic;

namespace PokerMetricsCore.Web.Models
{
    public class Player
    {
        // Modelo que representa o jogador
        public int PlayerId { get; set; }
        public string Name { get; set; } = null!; // "Iago Carvalho"

        public virtual ICollection<Arquivo> Arquivos { get; set; } = new List<Arquivo>(); // PN: 1 jogador pode dar upload de N arquivos
        public virtual ICollection<TorneioJogado> TorneiosJogados { get; set; } = new List<TorneioJogado>(); // PN: 1 jogador pode ter N torneios jogados
    }
}