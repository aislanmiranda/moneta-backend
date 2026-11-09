# moneta-backend

API do **Moneta** — app pessoal para centralizar despesas pessoais e de trabalho, lembretes de
saúde e agenda.

> **Visão geral, imagens e como rodar o projeto completo:**
> [moneta-frontend](https://github.com/aislanmiranda/moneta-frontend) — lá está o
> `docker-compose.yml` que sobe banco, API, scheduler e PWA.

## O que tem aqui

ASP.NET Core 10 + EF Core 10 (PostgreSQL), em camadas:

| Projeto | Papel |
|---|---|
| `Moneta.Domain` | Entidades e regras (módulos, lançamentos, itens, rotinas, usuários) |
| `Moneta.Application` | Serviços, DTOs e contratos de repositório |
| `Moneta.Infra` | `AppDbContext`, migrations e filtros por usuário |
| `Moneta.Repository` | Implementações dos repositórios |
| `Moneta.Api` | Controllers REST, autenticação JWT e SignalR (`/hubs/notifications`) |

Principais recursos: contas com login (senha de 6 dígitos, bloqueio após tentativas erradas),
isolamento de dados por usuário via filtros globais do EF, módulos Financeiro / Vida Saudável /
Agenda, lançamentos com recorrência, observação e comprovantes, rotinas executadas pelo
[moneta-schedule](https://github.com/aislanmiranda/moneta-schedule) e notificações em tempo real
(Postgres `LISTEN/NOTIFY` → SignalR).

Detalhes por funcionalidade em [`docs/`](docs/).

## Rodando só a API

```bash
dotnet user-secrets set "Auth:JwtKey" "$(openssl rand -base64 48)" --project src/Moneta.Api
dotnet ef database update --project src/Moneta.Infra --startup-project src/Moneta.Api
dotnet run --project src/Moneta.Api   # http://localhost:5080
```

A connection string padrão (`appsettings.json`) aponta para `localhost:5432`, banco/usuário/senha
`moneta` — ajuste em `ConnectionStrings__Moneta` se precisar.

## Licença

[MIT](LICENSE)
