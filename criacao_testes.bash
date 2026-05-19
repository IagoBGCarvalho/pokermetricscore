# Navegar para a pasta src
cd src

# Criar o novo projeto de testes usando o template do xUnit para .NET 10
dotnet new xunit -n PokerMetricsCore.Tests -f net10.0

# Voltar para a raiz para adicionar o projeto de testes à solução principal
cd ..
dotnet sln PokerMetricsCore.slnx add src/PokerMetricsCore.Tests/PokerMetricsCore.Tests.csproj

# Fazer o projeto de testes "enxergar" o projeto principal adicionado a referência a ele
cd src/PokerMetricsCore.Tests
dotnet add reference ../PokerMetricsCore.Web/PokerMetricsCore.Web.csproj