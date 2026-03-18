# Fiap CloudGames — Catalog API

API de catálogo do ecossistema CloudGames (FIAP). Responsável pela gestão de jogos, categorias, pedidos de compra e biblioteca de jogos do usuário, com integração a filas (RabbitMQ) para processamento assíncrono de pagamentos.

## Funcionalidades

- CRUD de jogos.
- CRUD de categorias.
- Criação de pedidos de compra com publicação de evento em fila RabbitMQ.
- Consumo de evento de pagamento processado e atualização do status do pedido.
- Adição automática de jogo à biblioteca do usuário após aprovação do pagamento.
- Consulta da biblioteca de jogos do usuário.
- Cancelamento de pedido.
- Autenticação via Firebase (JWT Bearer).
- Autorização por roles (Admin e User).
- Documentação OpenAPI com Swagger.
- Observabilidade com New Relic e logs estruturados via Serilog.

## Tecnologias

- .NET 10 — ASP.NET Core Web API
- PostgreSQL — persistência (Entity Framework Core)
- RabbitMQ — mensageria (MassTransit)
- MediatR — CQRS (commands/queries)
- FluentValidation — validação de comandos
- Swagger/OpenAPI (Swashbuckle) — documentação da API
- Firebase Authentication — autenticação via JWT Bearer
- New Relic — observabilidade (atributos customizados, erros, transações HTTP e MassTransit)
- Serilog — logs estruturados
- xUnit — testes unitários

## Estrutura da solução

```
src/
├── FiapCloudGames.Catalog.Api              # Web API, controllers, middlewares, configuração
├── FiapCloudGames.Catalog.Application      # Casos de uso, CQRS, DTOs, validadores
├── FiapCloudGames.Catalog.Domain           # Entidades, enums, regras de negócio, contratos
├── FiapCloudGames.Catalog.Infrastructure   # EF Core, repositórios, migrations
├── FiapCloudGames.Catalog.Observability    # New Relic (middleware HTTP, atributos customizados)
├── FiapCloudGames.Catalog.Shared           # Utilitários e contratos compartilhados
├── FiapCloudGames.Catalog.Tests            # Testes unitários (xUnit)
└── FiapCloudGames.Queue                    # Configuração de filas (RabbitMQ), publishers e consumers
```

## Pré-requisitos

- .NET 10 SDK
- PostgreSQL (local ou container)
- RabbitMQ (local ou container)

## Executando localmente

1. **Restaurar e buildar**

```bash
dotnet restore
dotnet build
```

2. **Configurar conexão e RabbitMQ**

Edite `src/FiapCloudGames.Catalog.Api/appsettings.Development.json` (ou use variáveis de ambiente):
- `ConnectionStrings:DefaultConnection` — connection string do PostgreSQL
- `RabbitmqSettings` — host, porta, virtual host, usuário e senha

3. **Aplicar migrations**

A API aplica migrations na inicialização (via `MigrationConfig`). Certifique-se de que o banco exista e a connection string esteja correta.

4. **Subir a API**

```bash
cd src/FiapCloudGames.Catalog.Api
dotnet run
```

## Docker

```bash
docker build -f docker/Dockerfile -t projetofiap/catalog-api:latest .
```

```bash
docker run -d -p 8083:8083 --name catalog-api projetofiap/catalog-api:latest
```

- A imagem expõe a porta `8083` (`EXPOSE 8083`), então o mapeamento recomendado é `-p 8083:8083`.
- O Dockerfile executa `dotnet FiapCloudGames.Catalog.Api.dll`.
- A imagem inclui o agente New Relic para .NET; configure `NEW_RELIC_LICENSE_KEY` e `NEW_RELIC_APP_NAME` no ambiente para enviar dados ao dashboard.

## Testes

Rodar todos os testes:

```bash
dotnet test src/FiapCloudGames.Catalog.Tests/FiapCloudGames.Catalog.Tests.csproj
```

Com cobertura (se tiver coverlet configurado):

```bash
dotnet test src/FiapCloudGames.Catalog.Tests/FiapCloudGames.Catalog.Tests.csproj --collect:"XPlat Code Coverage"
```

## Licença

Uso acadêmico / FIAP CloudGames.
