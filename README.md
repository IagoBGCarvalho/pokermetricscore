# Poker Metrics Core

## 📊 Sobre o Projeto

Sistema desenvolvido para automatizar a análise de desempenho em torneios de poker online. O sistema processa extratos detalhados (.xlsx, .txt, etc) fornecidos pelas plataformas e gera relatórios com métricas de performance por torneio, incluindo ROI, lucro líquido e estatísticas de jogo.

Recentemente refatorado de MVC para **Blazor Server**, o projeto agora oferece uma experiência mais flúida e interativa, com processamento em tempo real e lógica aprimorada para identificação de torneios.

## 🎯 Objetivo

Automatizar o processo de análise de desempenho semanal/mensal que anteriormente era feito manualmente, proporcionando insights rápidos e precisos sobre os torneios mais lucrativos, eliminando erros de cálculo e inconsistências de fuso horário.

## 🚀 Funcionalidades

- **Configuração Zero (Plug & Play)**: Distribuído no formato Self-Contained (.exe para Windows e .bin para Linux), a ferramenta não exige a instalação prévia do SDK do .NET pelo usuário final. O banco de dados é gerado automaticamente na primeira execução da aplicação. 
- **Upload via SignalR**: Processamento rápido de arquivos .xlsx utilizando streams de memória, sem necessidade de reload de página.
- **Análise de Performance**: Cálculo de ROI, lucro líquido e estatísticas por torneio.
- **Algoritmo de Matching Inteligente**:
  - **Correção de Fuso**: Conversão automática de UTC para Horário de Brasília.
  - **Lógica Circular**: Identificação precisa de torneios da madrugada (ex: jogos às 02:00 pertencentes à grade das 22:00).
- **Relatórios Interativos**: Visualização imediata dos resultados ordenados por rentabilidade.
- **Gestão de Dados**: Botão para limpeza do banco de dados antes de novos uploads.

## 🛠️ Tecnologias Utilizadas

- **.NET 10** - Framework principal
- **Blazor Web App (Interactive Server)** - Arquitetura web com renderização no servidor
- **Entity Framework Core** - ORM com suporte a `IDbContextFactory` para concorrência
- **SQLite** - Banco de dados local autogerenciado
- **ClosedXML** - Processamento robusto de arquivos Excel
- **Bootstrap** - Interface de usuário responsiva

## 📁 Estrutura do Projeto

```
PokerMetricsCore.Web/ 
├── Components/ 
│ ├── Layout/ 
│ │ ├── MainLayout.razor 
│ │ └── NavMenu.razor 
│ └── Pages/ 
│ ├── Upload.razor # Página Inicial
│ └── Reports.razor # Visualização dos Relatórios 
├── Models/ 
│ ├── Player.cs 
│ ├── DefinicaoTorneio.cs 
│ ├── TorneioJogado.cs 
│ ├── Transacao.cs 
│ ├── Arquivo.cs 
│ └── PerformanceTorneioDto.cs 
├── Services/ 
│ ├── ProcessamentoArquivoService.cs 
│ └── RelatorioService.cs 
├── Data/ 
│ └── PokerMetricsCoreContext.cs 
├── wwwroot/ 
└── Program.cs
```

## 🔄 Fluxo da Aplicação

1. **Inicialização**: Ao rodar o executável ou o binário, o sistema constrói o banco de dados `pokermetrics.db` automaticamente e levanta o servidor local.
2. **Upload de Extrato** (domínio:porta): Leitura do arquivo .xlsx via stream segura (SignalR).
   - O sistema ignora metadados do cabeçalho e busca a âncora de dados ("Date").
   - Aplica conversão de timezone e matemática modular para matching de torneios.
3. **Persistência**: Dados salvos em SQLite com verificação de duplicidade.
4. **Relatório** (`/Reports`): Navegação automática para a visualização de ROI e Lucro Líquido.

## 📊 Métricas Calculadas

- **ROI (Return on Investment)**: Retorno percentual sobre o investimento.
- **Resultado Líquido**: Lucro/prejuízo total por torneio (Payout - BuyIn).
- **Total de Entradas**: Quantidade de vezes que cada torneio foi jogado.

## 🎮 Como Usar

### 1. Obter Extrato da Ignition Poker
- Solicitar extrato semanal/mensal na plataforma Ignition Poker.
- Download do arquivo .xlsx com todas as transações.

### 2. Download
Na aba de Releases do repositório do projeto no github (https://github.com/IagoBGCarvalho/sentinela/releases) estão disponíveis os pacotes Self-Contained para Windows (pasta zip com .exe) e para Linux-x64 (.bin).

#### 2.1 Windows
Basta descompactar o arquivo zip e dar um duplo clique no executável. O sistema irá abrir um terminal, criar o banco e subir o servidor local no console, basta clicar no endereço (http://localhost:5000) com a tecla "CTRL" pressionada que uma janela do navegador irá se abrir com o sistema rodando.

#### 2.2 Linux
- Dar permissão de execução para o arquivo binário
```bash
chmod +x PokerMetricsCore.Web
```

- Executar a aplicação
```bash
./PokerMetricsCore.Web
```

### 3. Upload no Sistema
- Acessar a página inicial (padrão: http://localhost:5000).
- (Opcional) Clicar em **"⚠️ Limpar Banco de Dados"** para resetar análises anteriores do banco de dados.
- Selecionar arquivo .xlsx.
- Aguardar o processamento automático.

### 4. Analisar Resultados
- O sistema redireciona automaticamente para `/reports`.

## ⚙️ Configuração e Execução Para Desenvolvedores

### Pré-requisitos
- .NET 10 SDK
- Visual Studio 2022 ou VS Code
- Pacotes: Microsoft.EntityFrameworkCore, Microsoft.EntityFrameworkCore.Sqlite e Microsoft.EntityFrameworkCore.Design

### Execução Local
Recomendado o uso de Linux ou Windows.

```bash
git clone <repositorio>
cd src
cd PokerMetricsCore.Web/
dotnet restore
dotnet run
```

## 📈 Exemplo de Saída

| Torneio | Jogos | Resultado Líquido | ROI |
|---------|-------|------------------|-----|
| MICRO ROLLER | 15 | R$ 210,18 | 254,76% |
| MICRO NIGHTLY | 13 | R$ 90,90 | 127,13% |
| NIGHTLY | 1 | R$ 39,50 | 71,82% |

## 🔍 Casos de Uso

- **Análise Semanal**: Identificar torneios com melhor performance recente
- **Otimização de Bankroll**: Direcionar recursos para torneios mais rentáveis
- **Ajuste de Estratégia**: Baseado em dados concretos de ROI
- **Tracking de Progresso**: Monitorar evolução ao longo do tempo

## 📄 Licença

GNU GENERAL PUBLIC LICENSE Version 3, 29 June 2007

## 👥 Desenvolvimento

Desenvolvido por Iago Batista Gomes de Carvalho.