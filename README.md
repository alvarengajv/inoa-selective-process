# StockQuoteAlert

Aplicacao console em .NET que monitora cotacoes de acoes da bolsa em tempo real e envia alertas por e-mail quando o preco cruza limites de compra ou venda definidos pelo usuario.

![.NET](https://img.shields.io/badge/.NET-10.0-purple)
![C#](https://img.shields.io/badge/C%23-13-blue)

## Funcionalidades

- Monitoramento continuo de cotacoes via Yahoo Finance API
- Alertas por e-mail (SMTP/MailKit) ao cruzar limites de compra ou venda
- Mecanismo anti-spam: evita alertas duplicados enquanto o preco permanece na mesma zona
- Reset automatico dos alertas quando o preco retorna a faixa normal
- Intervalo de consulta configuravel
- Encerramento gracioso com `Ctrl+C`

## Arquitetura

O projeto segue **Clean Architecture** com separacao clara de responsabilidades:

```
src/
├── StockQuoteAlert.Domain/            # Entidades e regras de negocio
│   └── Entities/
│       └── MonitoredAsset.cs
│
├── StockQuoteAlert.Application/       # Interfaces, DTOs e Serviços
│   ├── DTOs/
│   ├── Interfaces/
│   │   ├── IMonitoringService.cs
│   │   ├── IQuoteService.cs
│   │   └── IEmailService.cs
│   └── Services/
│       └── MonitoringService.cs
│
├── StockQuoteAlert.Infrastructure/    # Integracoes externas
│   ├── Clients/
│   │   └── YahooFinanceClientAdapter.cs
│   └── ExternalServices/
│       ├── QuoteService.cs
│       └── EmailService.cs
│
├── StockQuoteAlert.Console/           # Ponto de entrada e DI
│   ├── Program.cs
│   ├── ConsoleWriter.cs
│   ├── Extensions/
│   │   └── ServiceCollectionExtensions.cs
│   └── appsettings.json
│
└── StockQuoteAlert.Tests/             # Testes unitarios
    ├── Entities/
    └── Services/
```

## Tecnologias

| Tecnologia | Versao | Uso |
|---|---|---|
| .NET | 10.0 | Runtime e framework |
| YahooFinanceApi | 2.3.3 | Cotacoes em tempo real |
| MailKit | 4.15.1 | Envio de e-mails via SMTP |
| xUnit | 2.9.3 | Testes unitarios |
| Moq | 4.20.72 | Mocking |
| FluentAssertions | 6.12.2 | Assertions fluentes |

## Pre-requisitos

- [.NET SDK 10.0](https://dotnet.microsoft.com/) ou superior

## Configuracao de Credenciais

As credenciais de e-mail **nao devem ser salvas no** `appsettings.json`. Use uma das opcoes abaixo:

### Opcao 1: .NET Secret Manager (recomendado para desenvolvimento)

```bash
cd src/StockQuoteAlert.Console

dotnet user-secrets set "EmailSettings:SmtpUsername" "seu_email@gmail.com"
dotnet user-secrets set "EmailSettings:SmtpPassword" "sua_senha_de_aplicativo"
dotnet user-secrets set "EmailSettings:RecipientEmail" "email_destino@gmail.com"
```

> **Nota:** Para Gmail, e necessario criar uma [Senha de Aplicativo](https://support.google.com/accounts/answer/185833?hl=pt-BR) nas configuracoes de seguranca da conta. A senha comum da conta nao funcionara.

### Opcao 2: Variaveis de Ambiente (recomendado para CI/CD e producao)

**Linux/macOS:**
```bash
export EmailSettings__SmtpUsername="seu_email@gmail.com"
export EmailSettings__SmtpPassword="sua_senha_de_aplicativo"
export EmailSettings__RecipientEmail="email_destino@gmail.com"
```

**Windows (PowerShell):**
```powershell
$env:EmailSettings__SmtpUsername="seu_email@gmail.com"
$env:EmailSettings__SmtpPassword="sua_senha_de_aplicativo"
$env:EmailSettings__RecipientEmail="email_destino@gmail.com"
```

## Como Executar

```bash
cd src/StockQuoteAlert.Console

dotnet run -- <ticker> <limite_venda> <limite_compra>
```

**Exemplo:**
```bash
dotnet run -- PETR4.SA 30.50 28.00
```

| Argumento | Descricao |
|---|---|
| `ticker` | Simbolo do ativo (ex: `PETR4.SA`, `VALE3.SA`) |
| `limite_venda` | Preco acima do qual dispara alerta de venda |
| `limite_compra` | Preco abaixo do qual dispara alerta de compra |

O intervalo de consulta e configuravel em `appsettings.json` pelo campo `MonitoringSettings:IntervalSeconds` (padrao: 30s).

## Testes

```bash
# Rodar todos os testes
dotnet test

# Com output detalhado
dotnet test -v normal

# Com cobertura de codigo
dotnet test /p:CollectCoverage=true
```

Os testes cobrem:
- **Domain** - Criacao, validacao e logica de alertas do `MonitoredAsset`
- **MonitoringService** - Fluxos de compra/venda, anti-spam e resiliencia
- **EmailService** - Validacao de parametros e configuracao SMTP
- **QuoteService** - Integracao com Yahoo Finance e tratamento de erros
