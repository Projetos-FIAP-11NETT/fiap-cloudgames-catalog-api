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

## Deploy (CI/CD — GitHub Actions)

O deploy é automatizado por 3 workflows em `.github/workflows/`, que juntos implementam um fluxo **GitHub Flow com `develop`**: toda mudança nasce numa branch de feature, vira PR pra `develop`, e o deploy real é disparado por uma **tag de versão**.

| Workflow | Dispara em | O que faz |
|---|---|---|
| `01-ci-push.yml` | `push` em `feature/**`, `bugfix/**` ou `hotfix/**` | Build + testes; se não existir PR aberto para `develop` a partir dessa branch, cria um automaticamente (via `gh pr create`) |
| `02-ci-pull-request.yml` | PR aberto/atualizado contra `develop` ou `main` | *Quality gate*: build, testes com cobertura, checagem de pacotes NuGet vulneráveis (**bloqueia** o merge) e desatualizados (apenas aviso) |
| `03-cd-release.yml` | `push` de tag `v*` | Build + testes, build da imagem Docker, publica no ECR e faz deploy no EKS |

### Passo a passo — do código à produção

**1. Criar a branch de feature a partir de `develop`:**

```bash
git checkout develop
git pull origin develop
git checkout -b feature/minha-mudanca
```

**2. Commitar e dar push:**

```bash
git add .
git commit -m "feat: minha mudança"
git push -u origin feature/minha-mudanca
```

Isso dispara o `01-ci-push.yml`: builda, roda os testes e — se ainda não existir — abre automaticamente um Pull Request de `feature/minha-mudanca` para `develop`.

**3. Revisão e merge:**

A cada push no PR, o `02-ci-pull-request.yml` roda o quality gate (build, testes + cobertura, vulnerabilidades). Só faz merge do PR em `develop` depois dele passar.

**4. Gerar e enviar a tag de release** (isso é o que efetivamente dispara o deploy):

```bash
git checkout develop
git pull origin develop
git tag v1.4.0
git push origin v1.4.0
```

> A tag **precisa apontar para um commit que já esteja em `develop`** — o `03-cd-release.yml` valida isso (`git merge-base --is-ancestor`) e falha se a tag não pertencer à branch.

O `03-cd-release.yml` então:
1. Builda e testa a aplicação.
2. Builda a imagem Docker, marcando com a tag da versão (`$IMAGE_TAG`) **e** com `latest`.
3. Autentica no ECR com as credenciais AWS (Academy) configuradas como secrets do repositório e faz o push das duas tags.
4. Roda o scanner **Trivy** na imagem (severidade HIGH/CRITICAL — hoje não bloqueia o pipeline).
5. Faz checkout do repositório `fiap-cloudgames-infrastructure` (branch `feature/fase-4`), conecta o `kubectl` ao cluster EKS e aplica os manifests em `k8s/catalog/api`.
6. Executa `kubectl rollout restart deployment/catalog-deployment` e aguarda o rollout — como o manifest referencia a imagem sem tag explícita (equivale a `:latest`), o restart força o pod a repuxar a imagem recém-publicada.

> **AWS Academy:** como as credenciais de sessão expiram, os secrets `AWS_ACCESS_KEY_ID` / `AWS_SECRET_ACCESS_KEY` / `AWS_SESSION_TOKEN` do repositório (Settings → Secrets and variables → Actions) precisam ser atualizados a cada sessão de lab antes de enviar uma nova tag — senão o job `publish-image`/`deploy-eks` falha na autenticação.

Todos os workflows também podem ser disparados manualmente (`workflow_dispatch`) na aba **Actions** do GitHub.

---

## Testes

```bash
dotnet test src/FiapCloudGames.Catalog.Tests
```

Cobertura gerada com **coverlet**. Os testes cobrem handlers de aplicação, entidades de domínio e consumers de fila.
