# FiapCloudGames — Catalog API

API REST para gerenciamento do catálogo de jogos da plataforma FiapCloudGames. Responsável por categorias, jogos, pedidos e biblioteca do usuário.

---

## Tecnologias

| Camada | Tecnologia |
|---|---|
| Runtime | .NET 10 / ASP.NET Core |
| Autenticação | JWT Bearer + Firebase |
| Banco de dados | PostgreSQL (EF Core) · MongoDB · Redis · Elasticsearch |
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
| Busca avançada | `GET /api/v1/game/search` | Busca fuzzy por título/descrição via **Elasticsearch** |
| Catálogo | `GET /api/v1/game/catalog` | Listagem paginada do catálogo |
| Pedidos | `GET/POST /api/v1/order` | Compra e consulta de pedidos |
| Biblioteca | `GET /api/v1/library` | Jogos adquiridos pelo usuário |

Documentação interativa disponível em `/swagger` quando em ambiente de desenvolvimento.

---

## Busca avançada (Elasticsearch)

O endpoint `/api/v1/game/search` roda **independente** da consulta relacional (Postgres), usando um índice dedicado no Elasticsearch com tolerância a erro de digitação (fuzzy search).

- **Cliente:** `Elastic.Clients.Elasticsearch` (client oficial).
- **Indexação assíncrona:** os comandos `Create`/`Update`/`Delete` de jogo publicam eventos (`IGameUpserted` / delete) via MassTransit; o `GameIndexConsumer` (em `FiapCloudGames.Queue`) consome o evento e atualiza o documento no índice (`GameSearchRepository` / `GameDocument`), mantendo o índice sincronizado sem acoplar a escrita no Postgres à disponibilidade do Elasticsearch.
- **Configuração:** `ElasticsearchSettings` (`Uri`, `IndexName`) via `appsettings` / variáveis de ambiente — sem credenciais hardcoded.

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

### Configuração de credenciais

O projeto **não versiona variáveis de ambiente nem segredos**. Os valores sensíveis no `appsettings.json` ficam vazios e devem ser preenchidos localmente. Use o `appsettings.Example.json` como referência da estrutura e das chaves esperadas.

---

## Testes

```bash
dotnet test src/FiapCloudGames.Catalog.Tests
```

Cobertura gerada com **coverlet**. Os testes cobrem handlers de aplicação, entidades de domínio e consumers de fila.
