# FiapCloudGames — Catalog API

API REST para gerenciamento do catálogo de jogos da plataforma FiapCloudGames. Responsável por categorias, jogos, pedidos e biblioteca do usuário.

---

## Tecnologias

| Camada | Tecnologia |
|---|---|
| Runtime | .NET 10 / ASP.NET Core |
| Autenticação | JWT Bearer + Firebase |
| Banco de dados | PostgreSQL (EF Core) · MongoDB · Redis |
| Mensageria | MassTransit + RabbitMQ / Amazon SQS |
| Observabilidade | Serilog · New Relic APM |
| Testes | xUnit · Moq · FluentAssertions |
| Container | Docker (porta 8083) |

---

## Estrutura do projeto

```
src/
├── FiapCloudGames.Catalog.Api           # Entry point — controllers, middleware, DI
├── FiapCloudGames.Catalog.Application   # CQRS com MediatR (commands/queries/handlers)
├── FiapCloudGames.Catalog.Domain        # Entidades e regras de domínio
├── FiapCloudGames.Catalog.Infrastructure# EF Core, repositórios, migrações
├── FiapCloudGames.Catalog.Observability # Logging, tracing, middlewares
├── FiapCloudGames.Catalog.Shared        # Abstrações compartilhadas
├── FiapCloudGames.Queue                 # Consumers e publishers MassTransit
└── FiapCloudGames.Catalog.Tests         # Testes unitários
```

---

## Endpoints principais

Todos os endpoints exigem autenticação JWT. Operações de escrita no catálogo são restritas à role **Admin**.

| Recurso | Rota base | Descrição |
|---|---|---|
| Categorias | `GET/POST/PUT/DELETE /api/v1/category` | CRUD de categorias |
| Jogos | `GET/POST/PUT/DELETE /api/v1/game` | CRUD de jogos |
| Pedidos | `GET/POST /api/v1/order` | Compra e consulta de pedidos |
| Biblioteca | `GET /api/v1/library` | Jogos adquiridos pelo usuário |

Documentação interativa disponível em `/swagger` quando em ambiente de desenvolvimento.

---

## Executando localmente

### Pré-requisitos

- .NET 10 SDK
- Docker + Docker Compose
- PostgreSQL na porta `5444`
- RabbitMQ ou credenciais AWS SQS configuradas

### Via Docker

```bash
docker build -t projetofiap/catalog-api:latest .
docker run -p 8083:8083 projetofiap/catalog-api:latest
```

### Via CLI

```bash
cd src/FiapCloudGames.Catalog.Api
dotnet run
```

As variáveis de conexão podem ser ajustadas em `appsettings.json` ou via variáveis de ambiente.

---

## Testes

```bash
dotnet test src/FiapCloudGames.Catalog.Tests
```

Cobertura gerada com **coverlet**. Os testes cobrem handlers de aplicação, entidades de domínio e consumers de fila.
