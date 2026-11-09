# Feature: Vida Saudável

**Status:** implementado ponta a ponta — backend (pessoas, itens, ocorrências) e frontend
(`/pessoal/vida-saudavel`) conversam com a API real, com lembretes em tempo real via o
mesmo mecanismo do Financeiro. Estrutura espelha deliberadamente
[financeiro-pessoal.md](financeiro-pessoal.md) — mesmo padrão, domínio diferente.

## Conceito

Três entidades, mesmo papel dos equivalentes do Financeiro:

| Vida Saudável | Papel | Equivalente em Financeiro |
|---|---|---|
| `HealthPerson` | pra quem é (Eu, Mãe, Filha...) | `CostCenter` |
| `HealthItem` | o remédio/água/academia cadastrado, já com a recorrência | `FinanceAccount` (+ recorrência) |
| `HealthLog` | uma ocorrência específica de um dia, com status | `FinanceEntry` |

**Decisão: `HealthPerson` é um cadastro próprio, não reaproveita `CostCenter`.** Mesmo
conceito de relacionamento (reaproveita o enum `CostCenterRelationship`, que já era
genérico o bastante), mas os dados são independentes — decisão explícita do usuário, não
uma limitação técnica.

**Decisão: `HealthItem` não tem "conta reaproveitável" como `FinanceAccount`.** Cada item
pertence a uma pessoa só — "Vitamina D" pra mim e pra minha mãe são dois itens separados,
não um item com lançamentos pra pessoas diferentes.

## Entidades

### HealthPerson

`Id`, `Name`, `Relationship` (`CostCenterRelationship` reaproveitado).

### HealthItem

| Campo | Tipo | Observação |
|---|---|---|
| `Title` | string | ex: "Vitamina D" |
| `Type` | `HealthItemType` | `Medication`/`Water`/`Gym` |
| `HealthPersonId` | FK | pra quem é |
| `Dosage` | string? | só preenchido quando `Type = Medication` |
| `Status` | `HealthItemStatus` | `Active`/`Paused` — controla se o worker gera ocorrências pra ele |
| `CronExpression`/`DaysOfWeek`/`Hours`/`Minute` | — | agendamento, **mesma forma de `Routine`**, flattened direto na linha (sempre 1:1 com o item, sem necessidade de tabela separada). A UI reaproveita `ScheduleField` (`frontend/src/components/schedule/ScheduleField.tsx`) e o backend reaproveita `CronBuilder.Resolve` (`Application/Routines/CronBuilder.cs`) — mesma composição/validação de cron usada nas rotinas. |

### HealthLog

`Id`, `HealthItemId`, `ScheduledAt`, `Status` (`Pending`/`Done`/`Missed`), `DoneAt`.
**Nunca criado pelo usuário** — só pelo `HealthLogGeneratorJob` (ver abaixo).

## Endpoints

```
GET    /api/healthpersons
POST   /api/healthpersons
DELETE /api/healthpersons/{id}

GET    /api/healthitems
POST   /api/healthitems              { title, type, healthPersonId, dosage?, schedule }
PUT    /api/healthitems/{id}
PATCH  /api/healthitems/{id}/status  { "status": "Paused" }
DELETE /api/healthitems/{id}

GET    /api/healthlogs?from=&to=     (default: hoje)
PATCH  /api/healthlogs/{id}/status   { "status": "Done" }
```

## Worker (`schedule/`) — dois jobs

**`HealthLogGeneratorJob`** — roda a cada 10 min (trigger fixo em `Program.cs`, fora do
mecanismo de rotinas/`ScheduleSyncJob` — é infraestrutura, não uma rotina
configurável). Pra cada `HealthItem` `Active`: se hoje bate com `DaysOfWeek` (ou é "todos
os dias"), garante um `HealthLog` `Pending` pra cada `Hour`, hoje — idempotente. Também
varre `Pending` com mais de 24h e marca `Missed`. Só funciona pra itens com agendamento
estruturado (`Hours` preenchido) — itens em "modo avançado" (cron cru) são ignorados
aqui, decomposição genérica de cron não foi implementada.

**`HealthReminderJob`** — como o `FinanceReminderJob`: disparado por uma rotina
(`Routine`, `Type = TimeControlReminder`), busca `HealthLog`s `Pending` com `ScheduledAt` já passado,
agrupa por `HealthItem.Type` e gera até 3 `Notification`s por execução
(`"2 remédios pendentes: Vitamina D, Omeprazol"`) com `pg_notify` — chega no sino/toast
pelo mesmo caminho do Financeiro (`notificacoes-realtime.md`), zero mudança lá.

**Bug real encontrado e corrigido durante a validação:** `DateTimeOffset.Now` tem offset
local (ex: -03:00 no Brasil) — o Npgsql rejeita isso pra colunas `timestamptz`, só aceita
offset 0 (UTC). Os três pontos que construíam/comparavam `DateTimeOffset` local contra o
Postgres (`HealthLogGeneratorJob`, `HealthReminderJob`, `HealthLogRepository`) precisaram
de `.ToUniversalTime()` antes de qualquer uso em query/escrita — a lógica de "que hora
local é essa" continua correta, só a representação enviada ao Postgres muda.

## Frontend

- **`pages/health/Health.tsx`** — espelha `pages/finance/Finance.tsx`: stat cards
  (Pendentes hoje, Feitos hoje, Perdidos — tom `"health"` novo em `StatCard`), chips de
  pessoas cadastradas, tabela "Hoje" (ocorrências do dia, "Marcar como feito") e tabela
  "Itens cadastrados" (CRUD, recorrência via `describeSchedule` reaproveitado de
  `constants/frequencyPresets.ts`).
- **`NewHealthItemModal.tsx`/`EditHealthItemModal.tsx`** — reaproveitam `ScheduleField`
  igual às rotinas. O modal de criação permite cadastrar uma pessoa nova inline (sem
  precisar de uma tela separada de gestão de pessoas).
- **Rotinas** — para receber lembretes de Vida Saudável, crie uma rotina apontando para o
  módulo ("Rotinas" → "Nova rotina"). Os detalhes (remédio, dosagem, pessoa) vivem no
  item, não na rotina.

## Cor vermelha (identidade da área)

Nova variável `--color-health` em `theme.css` (claro e escuro) — tom vermelho distinto de
`--color-danger` na intenção (identidade de área, não erro), aplicada em:

- Item "Vida Saudável" do menu lateral (`navConfig.ts` ganhou `accent?: "health"`,
  `Sidebar.css` colore ícone/texto mesmo fora do estado ativo).
- Botão "Novo item" e "Marcar como feito" na tela de Saúde (`.btn--health`).
- Badge do tipo "Lembrete de saúde" na tabela de Atividades
  (`.activity-table__type--health`).

Resto do layout (fundo, tipografia, cards, modais) continua no tema padrão — só esse
acento muda.

## Pendências

- [ ] **Volume/quantidade de água.** Só lembrete de horário, não registra "quantos ml bebi".
- [ ] **Escalonamento de dose perdida.** `Missed` só muda o status, sem alerta diferenciado.
- [ ] **Itens em modo avançado não geram `HealthLog`.** `HealthLogGeneratorJob` só lê
  `DaysOfWeek`/`Hours` — um item com cron cru fica sem ocorrências até isso ser tratado.
