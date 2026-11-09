# Arquitetura do backend

O backend é uma solution .NET 10 (`AgentOffice.slnx`) dividida em 5 projetos, seguindo
Clean Architecture / Onion Architecture. A regra de dependência é sempre "de fora para
dentro" — nada no centro (`Domain`) conhece as camadas externas.

```
src/
  AgentOffice.Domain/        núcleo: entidades, enums, exceções de domínio. Sem dependências externas.
  AgentOffice.Application/   casos de uso: DTOs, interfaces de repositório, services. Depende só de Domain.
  AgentOffice.Infra/         DbContext (EF Core), migrations, seed. Depende de Domain.
  AgentOffice.Repository/    implementações EF Core das interfaces de repositório. Depende de Domain + Application + Infra.
  AgentOffice.Api/           ASP.NET Core: controllers, Program.cs, middleware. Depende de Application + Repository + Infra (só para DI).
```

Grafo de dependências:

```
Domain  <--  Application  <--  Repository  <--  Api
   ^                               |
   +-------------- Infra ----------+
```

## Por que separar Repository de Infra

`Infra` guarda o que é puramente infraestrutura de persistência (DbContext, migrations,
seed inicial). `Repository` guarda as implementações concretas das interfaces que a
`Application` define (`IRoutineRepository`, `IDateControlEntryRepository`, etc.), usando o
`AppDbContext` do `Infra`. Essa separação existe para que `Application` nunca precise
referenciar EF Core diretamente — ela só conhece as abstrações.

Decisão consciente: **não usamos um `IRepository<T>` genérico.** Cada agregado tem sua
própria interface de repositório, com os `Include()` que ele realmente precisa (ex:
`FinanceEntryRepository` sempre inclui `FinanceAccount` e `CostCenter`). Um repositório
genérico tende a vazar detalhes do EF Core (eager loading, tracking) de um jeito que
atrapalha mais do que ajuda num projeto deste tamanho.

## Padrão por camada

- **Domain**: entidades são classes simples (não records) com os campos e um método
  `EnsureValid()` quando há regras cross-field (ex: `FinanceEntry`). Exceções de domínio
  (`DomainValidationException`, `NotFoundException`) vivem em `Domain/Exceptions` e são
  mapeadas para HTTP pela `ExceptionHandlingMiddleware` da Api — os controllers não têm
  `try/catch`.
- **Application**: um par `I{Nome}Service` / `{Nome}Service` por agregado, orquestrando
  repositório(s) e regras de aplicação (ex: resolver `FinanceAccountId`/`CostCenterId`
  antes de criar um `FinanceEntry`). DTOs são `record`s imutáveis, um arquivo
  `{Nome}Dtos.cs` por agregado.
- **Repository**: uma classe por agregado, injetando `AppDbContext`. Toda escrita
  (`AddAsync`/`UpdateAsync`/`RemoveAsync`) já chama `SaveChangesAsync` — não há
  `IUnitOfWork` separado; para o tamanho atual do projeto isso seria complexidade sem
  ganho real.
- **Api**: controllers finos — só roteiam para o service e traduzem `null`/`bool` em
  `404`/`204`. Program.cs registra cada camada via extension method
  (`AddInfra`, `AddRepositories`, `AddApplication`).

## Convenções

- Enums do domínio são serializados como string no JSON (`JsonStringEnumConverter`),
  nunca como número — facilita debug e evita quebra silenciosa se a ordem dos valores
  mudar.
- IDs são `Guid`, gerados na camada de aplicação (`Guid.NewGuid()`), nunca pelo banco.
- Migrations vivem em `Infra/Persistence/Migrations` e são geradas com `Api` como
  startup project (veja `docs/README.md` ou a seção de comandos abaixo).

## Comandos úteis

```bash
# build da solution inteira
dotnet build

# rodar a API (a partir de backend/)
dotnet run --project src/AgentOffice.Api

# nova migration
dotnet ef migrations add NomeDaMigration \
  --project src/AgentOffice.Infra --startup-project src/AgentOffice.Api \
  --output-dir Persistence/Migrations

# aplicar migrations
dotnet ef database update \
  --project src/AgentOffice.Infra --startup-project src/AgentOffice.Api
```
