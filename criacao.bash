# Cria o arquivo de solução na raiz do repositório
dotnet new sln -n PokerMetricsCore

mkdir src # Cria a pasta source
cd src

# Cria o projeto Blazor Web App com interatividade Server Global
dotnet new blazor -o PokerMetricsCore.Web --interactivity Server --all-interactive

# Adiciona à solução
dotnet sln PokerMetricsCore.slnx add src/PokerMetricsCore.Web/PokerMetricsCore.Web.csproj

cd src/PokerMetricsCore.Web

# Instala as dependências do projeto
dotnet add PokerMetricsCore.Web package Microsoft.EntityFrameworkCore
dotnet add PokerMetricsCore.Web package Microsoft.EntityFrameworkCore.Sqlite
dotnet add PokerMetricsCore.Web package Microsoft.EntityFrameworkCore.Design
dotnet add PokerMetricsCore.Web package ClosedXML

# Criando as pastas que o Blazor não cria por padrão:
mkdir PokerMetricsCore.Web/Data
mkdir PokerMetricsCore.Web/Services
mkdir PokerMetricsCore.Web/Models