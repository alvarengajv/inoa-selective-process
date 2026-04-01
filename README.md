# StockQuoteAlert

O **StockQuoteAlert** é uma aplicação de *console* desenvolvida em .NET (C#) destinada ao monitoramento contínuo das cotações de ativos listados na Bolsa de Valores. O sistema aciona alertas por e-mail informando o usuário sempre que o preço da ação cruzar determinados parâmetros parametrizados para compra ou venda.

A arquitetura do projeto espelha boas práticas de engenharia de software valendo-se principalmente dos conceitos fundamentais de *Clean Architecture*, promovendo o máximo isolamento de responsabilidades, testabilidade e separação cristalina de preocupações.

## Arquitetura
O projeto encontra-se subdividido nas seguintes diretrizes arquiteturais que favorecem a Inversão de Controle e o Princípio de Segregação (SOLID):

- **Domain:** Modelagem do domínio central. Encapsula o núcleo das regras de negócio atreladas a avaliações e checagem de limites operacionais para disparo. Exemplo: `MonitoredAsset`.
- **Application:** Intermediário isolado, rege os casos de uso. Mantenedor do *worker/loop* de orquestração através do seu respectivo provedor abstrato `MonitoringService`.
- **Infrastructure:** Camada alicerce responsável pelas comunicações externas e processamentos voláteis. Furtiva integradora que executa cotações (API Yahoo Finance) nativas e envio digital de e-mails através de instâncias de `SmtpClient` ou `MailKit`. 
- **Console:** A Interface de entrada interativa do programa. Despacha injeções de dependências (DI) conectando estritamente camadas essenciais sem desrespeitar os isolamentos, sendo de onde decorre primordialmente a comunicação verbal (I/O) direta com o cliente.

## Requisitos e Setup
O ambiente necessita ser configurado com recursos atrelados a tecnologia principal.
- [.NET SDK 10](https://dotnet.microsoft.com/) ou versões estritamente posteriores aderentes.

## Configuração Segura de Credenciais (E-mail)

Prezando por integridade e para reprimir vazamentos de informações críticas dentro de versionadores de controle (Git), **nunca escreva senhas ou e-mails literais e diretos no arquivo** estático base: `appsettings.json`.
Portanto, a aplicação impõe uma camada de injeção provinda de origens secundárias seguras de *secrets*. Para abastecer as suas propriedades em ambiente local proceda das seguintes formas estabelecidas.

### Opção 1: Utilizando o `.NET Secret Manager` (Aconselhado em Local/Dev)

Ferramenta recomendada oficial adotada para blindar desenvolvedores do armazenamento indevido dentro do projeto físico. 

A partir do diretório base do host principal (projeto _Console_), utilize o CLI para abastecer localmente a árvore oculta na sua máquina:
```bash
# Navegue até o diretório do console para registrar a instância:
cd src/StockQuoteAlert.Console

# Alimente suas credenciais particulares do provedor de envios (ex: Gmail, SendGrid):
dotnet user-secrets set "EmailSettings:SmtpUsername" "seu_email_real@gmail.com"
dotnet user-secrets set "EmailSettings:SmtpPassword" "sua_senha_de_aplicativo"
dotnet user-secrets set "EmailSettings:RecipientEmail" "seu_email_para_receber_notificacoes@gmail.com"
```
> **Aviso de Senha**: Ao operar com plataformas como Microsoft, Gmail ou Yahoo, comumente se faz estritamente obrigatório a confecção primária de uma [*Senha de Aplicativo (App Passwords)*](https://support.google.com/accounts/answer/185833?hl=pt-BR) autorizada pelo ecossistema de segurança (2FA) do seu e-mail de despache. A senha habitual da sua conta web regular habitualmente resultará em interrupções operacionais e impedimentos ("Unauthorized"). 

### Opção 2: Declaração de Variáveis de Ambiente (CI/CD / Produção)

Método de inserção recomendada à provedores que realizarão as publicações remotas (Ex: Docker, GitHub Actions, Azure). 

A declaração deve ser transposta unindo a classe de roteamento por intermédio de caractere duplo `__`:
- Instalações atreladas à arquitetura base **Windows** (`PowerShell`):
  ```powershell
  $env:EmailSettings__SmtpUsername="seu_email_real@gmail.com"
  $env:EmailSettings__SmtpPassword="sua_senha_de_aplicativo"
  ```
- Instalações atreladas à arquitetura base **Linux** ou **macOS** (`Bash`):
  ```bash
  export EmailSettings__SmtpUsername="seu_email_real@gmail.com"
  export EmailSettings__SmtpPassword="sua_senha_de_aplicativo"
  ```

## Como Executar

O comando construtor requer necessariamente obrigatoriedade das três frações iniciais (`Argumentos`): 

* `<ativo_ticker>` (Indica a ação na bolsa a ser consultada, ex: `PETR4.SA`) 
* `<limite_venda>` (Decimal)
* `<limite_compra>` (Decimal)

```bash
# Entrando na pasta Console da ferramenta:
cd src/StockQuoteAlert.Console

# Inicie utilizando os parâmetros
dotnet run -- PETR4.SA 30.50 28.00
```

> **Atenção:** A frequência do serviço agendador de laço (polling) obedece os temporizadores fixados diretamente pelo seu `appsettings.json` através do nodo estrutural `IntervalSeconds`, definindo de quanto em quanto tempo novas consultas e auditorias de limiares de alarmes intervirão.
