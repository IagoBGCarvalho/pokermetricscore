using PokerMetricsCore.Web.Models;

namespace PokerMetricsCore.Web.Services;

/// <summary>
/// Interface para definir as declarações de métodos do serviço de matching de torneios.
/// Facilita a injeção de dependência e a criação de mocks para testes automatizados.
/// </summary>
public interface IMatchingTorneiosService
{
    DefinicaoTorneio AcharTorneioMaisProximo(TimeOnly horarioAlvo, IEnumerable<DefinicaoTorneio> candidatos);
    double CalcularDistanciaCircular(TimeOnly horario1, TimeOnly horario2);
}

public class MatchingTorneiosService : IMatchingTorneiosService
{
    /// <summary>
    /// Acha o torneio mais próximo do horário alvo dentro de uma lista de candidatos, utilizando o algoritmo circular.
    /// </summary>
    /// <remarks>
    /// Utiliza o cálculo circular para ordenar a lista de candidatos, pegar o mais próximo do tempo alvo e retornar a sua definição em forma de objeto. Caso a lista de candidatos seja nula ou não contenha nenhum elemento, uma exceção é lançada.
    /// </remarks>
    /// <param name="horarioAlvo">O horário exato em que o buy-in do torneio foi registrado no extrato do jogador.</param>
    /// <param name="candidatos">Lista de objetos <c>DefinicaoTorneio</c> que possuem o mesmo valor de buy-in e são candidatos ao matching.</param>
    /// <returns>
    /// Objeto da classe <c>DefinicaoTorneio</c> que representa o torneio cujo horário de começo é o mais próximo ao horário alvo.
    /// </returns>
    /// <exception cref="ArgumentException">Lançada quando a lista de candidatos fornecida é nula ou vazia.</exception>
    public DefinicaoTorneio AcharTorneioMaisProximo(TimeOnly horarioAlvo, IEnumerable<DefinicaoTorneio> candidatos)
    {
        if (candidatos == null || !candidatos.Any())
            throw new ArgumentException("A lista de candidatos não pode ser nula ou vazia.", nameof(candidatos));

        return candidatos.OrderBy(def => CalcularDistanciaCircular(horarioAlvo, def.HorarioComeco)).First();
    }

    /// <summary>
    /// Calcula a menor distância em minutos entre dois horários, considerando o ciclo de 24 horas (1440 minutos) de um relógio.
    /// </summary>
    /// <remarks>
    /// A distância real é o menor valor entre a diferença linear (direta) e a diferença circular (a volta no relógio).
    /// Essencial para resolver ambiguidades de torneios que começam perto da meia-noite.
    /// </remarks>
    /// <param name="horario1">O primeiro horário a ser comparado.</param>
    /// <param name="horario2">O segundo horário a ser comparado.</param>
    /// <returns>A menor distância absoluta em minutos (tipo <c>double</c>) entre os dois horários informados.</returns>
    public double CalcularDistanciaCircular(TimeOnly horario1, TimeOnly horario2)
    {
        double diferencaMinutos = Math.Abs((horario1 - horario2).TotalMinutes);
        
        return Math.Min(diferencaMinutos, 1440 - diferencaMinutos);
    }
}