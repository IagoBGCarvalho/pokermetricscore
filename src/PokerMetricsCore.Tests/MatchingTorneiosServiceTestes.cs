using PokerMetricsCore.Web.Models;
using PokerMetricsCore.Web.Services;
using Xunit;

namespace PokerMetricsCore.Tests;

/// <summary>
/// Classe responsável por conter os testes de unidade do serviço <see cref="MatchingTorneiosService"/>.
/// </summary>
/// <remarks>
/// Os testes verificam a exatidão do cálculo de distância circular de horários e a lógica de seleção 
/// do torneio mais próximo, garantindo que o matching do extrato funcione adequadamente em cenários de virada de dia.
/// </remarks>
public class MatchingTorneiosServiceTests
{
    private readonly MatchingTorneiosService _service;

    /// <summary>
    /// Construtor da classe de testes.
    /// </summary>
    /// <remarks>
    /// Instancia o serviço a ser testado. Como a classe de testes do xUnit é instanciada novamente para cada método de teste,
    /// isso garante um estado limpo (fresh state) para cada execução.
    /// </remarks>
    public MatchingTorneiosServiceTests()
    {
        _service = new MatchingTorneiosService();
    }

    /// <summary>
    /// Testa o cálculo da distância circular provendo vários cenários de horários, incluindo viradas de dia.
    /// </summary>
    /// <param name="hora1Str">Primeiro horário no formato string (HH:mm).</param>
    /// <param name="hora2Str">Segundo horário no formato string (HH:mm).</param>
    /// <param name="distanciaEsperadaMinutos">O resultado em minutos que a matemática deve retornar.</param>
    [Theory] // Anotação/Atributo que identifica um teste que pode ser rodado várias vezes com dados diferentes. 
    [InlineData("10:00", "11:00", 60)]   // Distância linear simples
    [InlineData("23:55", "00:05", 10)]   // Virada do relógio (Late register logo após meia-noite)
    [InlineData("00:05", "23:55", 10)]   // Ordem inversa dos parâmetros não deve alterar a distância absoluta
    [InlineData("12:00", "00:00", 720)]  // Extremos opostos do relógio (12 horas)
    public void CalcularDistanciaCircular_DeveRetornarMenorDistanciaCorreta(string hora1Str, string hora2Str, double distanciaEsperadaMinutos)
    {
        // Arrange = Configurar as variáveis, instanciar as classes e preparar os dados fictícios que o teste vai usar
        var hora1 = TimeOnly.Parse(hora1Str);
        var hora2 = TimeOnly.Parse(hora2Str);

        // Act = Chamar efetivamente o método que se quer testar
        var resultado = _service.CalcularDistanciaCircular(hora1, hora2);

        // Assert = Comparar o resultado que o método devolveu com o resultado que se espera que ele devolva.
        Assert.Equal(distanciaEsperadaMinutos, resultado);
    }

    /// <summary>
    /// Testa se o algoritmo escolhe corretamente o torneio mais próximo dentro de uma lista de candidatos.
    /// </summary>
    /// <remarks>
    /// Simula um cenário onde um buy-in ocorre às 00:15 e verifica se o algoritmo consegue atrelá-lo 
    /// ao torneio das 23:30 (que está a 45 minutos de distância na volta do relógio), 
    /// ignorando os torneios da tarde do dia atual.
    /// </remarks>
    [Fact] // Anotação/Atributo que identifica um teste simples que não recebe parâmetros
    public void AcharTorneioMaisProximo_DeveRetornarTorneioComMenorDistanciaCircular()
    {
        // Arrange
        var horarioBuyIn = new TimeOnly(0, 15); // 00:15 da madrugada

        var torneiosCandidatos = new List<DefinicaoTorneio>
        {
            new DefinicaoTorneio { Nome = "Torneio Tarde", HorarioComeco = new TimeOnly(15, 0) },
            new DefinicaoTorneio { Nome = "Torneio Noite", HorarioComeco = new TimeOnly(20, 0) },
            new DefinicaoTorneio { Nome = "Torneio Madrugada", HorarioComeco = new TimeOnly(23, 30) } // Alvo correto
        };

        // Act
        var torneioVencedor = _service.AcharTorneioMaisProximo(horarioBuyIn, torneiosCandidatos);

        // Assert
        Assert.NotNull(torneioVencedor);
        Assert.Equal("Torneio Madrugada", torneioVencedor.Nome);
    }

    /// <summary>
    /// Verifica se o método de achar o torneio levanta a exceção correta quando recebe uma lista nula ou vazia.
    /// </summary>
    /// <exception cref="ArgumentException">A exceção esperada pelo xUnit.</exception>
    [Fact]
    public void AcharTorneioMaisProximo_ComListaVazia_DeveLancarExcecao()
    {
        // Arrange
        var horarioBuyIn = new TimeOnly(12, 0);
        var listaVazia = new List<DefinicaoTorneio>();

        // Act & Assert
        // O xUnit captura a exceção que ocorre dentro da expressão lambda e valida se é do tipo esperado.
        var excecao = Assert.Throws<ArgumentException>(() => 
            _service.AcharTorneioMaisProximo(horarioBuyIn, listaVazia));

        Assert.Contains("nula ou vazia", excecao.Message);
    }
}
