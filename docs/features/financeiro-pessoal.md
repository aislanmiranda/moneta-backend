# Feature: Financeiro Pessoal

**Status:** cadastro implementado ponta a ponta — backend (contas, centros de custo e
lançamentos) e frontend (`/pessoal/financeiro`) já conversam com a API real. Lembretes
automáticos de lançamentos pendentes/vencidos rodam via `schedule/` (Quartz) — ver
[Lembretes via rotinas](#lembretes-via-rotinas-schedule).

> Nomes de entidade nesta página são os da primeira versão (`FinanceEntry`, `CostCenter`...).
> No código atual o Financeiro é um módulo do padrão `DateControl` (`DateControlEntry`,
> `DateControlParty`, `DateControlSource`).

## Pessoal × Trabalho

Despesas pessoais e de trabalho ficam no mesmo módulo. Cada lançamento tem
`Scope` (`DateControlEntryScope`: `Personal` | `Work`, padrão `Personal`), escolhido em
"Tipo de despesa" ao criar/editar. A tela tem um filtro "Pessoal e trabalho / Pessoal /
Trabalho" que restringe tudo (cards de totais, abas e tabela); lançamentos de trabalho levam
a etiqueta "Trabalho" na descrição. Substitui a antiga seção "Trabalho" do menu.

## Conceito

Dois níveis, propositalmente separados:

1. **Conta (`FinanceAccount`)** — o cadastro de uma conta recorrente, ex: "Internet
   residencial". Tem uma descrição e uma categoria (`Category`), que define o ícone/tipo
   mostrado na UI.
2. **Lançamento (`FinanceEntry`)** — um "lançar conta" de fato: valor, vencimento,
   parcelas, recorrência, se é previsto, e para qual centro de custo. Uma `FinanceAccount`
   pode ter vários `FinanceEntry` ao longo do tempo (ex: a fatura de internet lançada
   mês a mês).

Essa separação existe porque "cadastrar a conta" e "lançar a conta" são ações distintas
no fluxo que o usuário descreveu, e porque uma mesma conta pode gerar lançamentos com
centro de custo ou valor diferentes em momentos diferentes.

## Entidades

### CostCenter (centro de custo)

Responde "pra quem é esse gasto". Tem um nome livre e um `Relationship` (enum) que
categoriza os casos comuns:

| Relationship | Uso |
|---|---|
| `Self` | "Eu mesmo" |
| `Father` | "Meu pai" |
| `Mother` | "Minha mãe" |
| `Spouse`, `Child` | outros membros da família |
| `Other` | qualquer centro de custo customizado |

Os três primeiros (`Self`/`Father`/`Mother`) são **semeados automaticamente** na
primeira vez que a API sobe em ambiente de desenvolvimento (`DbSeeder.SeedAsync`,
chamado em `Program.cs` só quando `Environment.IsDevelopment()`). Isso é um seed de
conveniência, não uma migration de dados — em produção isso deve ser revisado (ver
pendências).

### FinanceAccount (conta)

| Campo | Tipo | Observação |
|---|---|---|
| `Description` | string | ex: "Internet residencial" |
| `Category` | enum | `Internet`, `Energy`, `Water`, `CreditCard`, `Subscription`, `Rent`, `Other`, `Health`, `Leisure` — "o tipo da conta". `Health`/`Leisure` foram acrescentados **depois** dos demais de propósito: o enum é salvo como inteiro no Postgres, então novos valores sempre vão no fim, nunca no meio (deslocaria os valores já gravados). |

### FinanceEntry (lançamento)

| Campo | Tipo | Observação |
|---|---|---|
| `FinanceAccountId` | FK | conta à qual este lançamento pertence |
| `CostCenterId` | FK | pra quem é este lançamento |
| `Amount` | decimal(12,2) | valor |
| `DueDate` | date | vencimento |
| `InstallmentsCount` | int? | **parcelas**: em quantas vezes esta compra é dividida (1 ou null = à vista) |
| `RecurrenceType` | enum | **recorrência**: `None`, `FixedCount` (repete N vezes) ou `Indeterminate` (repete sem fim definido) |
| `RecurrenceCount` | int? | obrigatório quando `RecurrenceType = FixedCount`; proibido nos outros casos |
| `IsForecast` | bool | se é um lançamento **previsto** (planejamento futuro, não uma conta real ainda) |
| `ForecastStartDate` / `ForecastEndDate` | date? | obrigatório (início) quando `IsForecast = true`; proibido quando `false` |
| `Status` | enum | `Pending`, `Paid`, `Overdue` — criado sempre como `Pending` |
| `OccurrenceNumber` | int? | posição (1-based) deste lançamento dentro da série que o gerou — ver [Geração de séries](#geração-de-séries-parcelas-e-recorrência-fixa) abaixo. `null` quando o lançamento é único. |

**Parcelas vs. recorrência não são a mesma coisa** e o modelo permite os dois
simultaneamente de propósito:
- *Parcelas* dividem uma compra única em N pagamentos (ex: "TV em 10x").
- *Recorrência* repete a mesma cobrança ao longo do tempo (ex: assinatura mensal).

### Geração de séries (parcelas e recorrência fixa)

Quando o `POST /api/financeentries` recebe `InstallmentsCount > 1` **ou**
`RecurrenceType = FixedCount`, o `FinanceEntryService.CreateAsync` não grava só o
lançamento enviado — ele gera a série inteira de uma vez: N linhas de `FinanceEntry`
independentes, uma por mês (`DueDate.AddMonths(i)`), cada uma com seu `OccurrenceNumber`
(1, 2, 3...). O endpoint continua devolvendo um único `FinanceEntryDto` (o primeiro da
série) — a resposta não muda de formato, então o frontend não precisou de ajuste no
client, só passou a exibir `OccurrenceNumber` (`describeRepetition()` mostra "2/3" em vez
do genérico "Recorrente (3x)").

Cada linha gerada é **independente**: não existe um `SeriesId` compartilhado, então pagar,
editar ou excluir uma ocorrência não afeta as outras. Isso é uma limitação conhecida (ver
Pendências) — ainda não dá pra "excluir a série toda" de uma vez.

`RecurrenceType = Indeterminate` continua **sem** geração automática: não tem fim definido,
então não há quantidade pra gerar de antemão. Esse caso segue precisando do scheduler
pra ir criando as ocorrências conforme o tempo passa.

### Validação (`FinanceEntry.EnsureValid()`)

Regras cross-field que não cabem em constraint de coluna, verificadas no domínio antes
de salvar (chamadas por `FinanceEntryService.CreateAsync`, erro vira HTTP 400):

- `Amount` deve ser > 0.
- `InstallmentsCount`, se informado, deve ser ≥ 1.
- `RecurrenceType = FixedCount` ⇒ `RecurrenceCount` obrigatório e ≥ 1.
- `RecurrenceType ≠ FixedCount` ⇒ `RecurrenceCount` deve ser nulo.
- `IsForecast = true` ⇒ `ForecastStartDate` obrigatório; `ForecastEndDate`, se
  informado, não pode ser anterior ao início.
- `IsForecast = false` ⇒ `ForecastStartDate`/`ForecastEndDate` devem ser nulos.

## Endpoints

```
GET    /api/costcenters
POST   /api/costcenters
DELETE /api/costcenters/{id}

GET    /api/financeaccounts
GET    /api/financeaccounts/{id}
POST   /api/financeaccounts
PUT    /api/financeaccounts/{id}
DELETE /api/financeaccounts/{id}

GET    /api/financeentries
GET    /api/financeentries/{id}
POST   /api/financeentries
PUT    /api/financeentries/{id}          valor, vencimento, centro de custo, previsto
PATCH  /api/financeentries/{id}/status   { "status": "Paid" }
DELETE /api/financeentries/{id}
```

`PUT .../financeentries/{id}` é deliberadamente mais restrito que o `POST`: não dá pra
mudar descrição/categoria (pertencem à `FinanceAccount`, que pode estar compartilhada
por várias ocorrências da mesma série — editar renomearia todas de uma vez) nem
parcelas/recorrência (isso é instrução de geração, não uma propriedade da ocorrência já
criada). Dá pra editar valor, vencimento, centro de custo e os campos de previsto.

### Exemplo: criar uma conta e lançar a primeira parcela

```bash
# 1. cadastrar a conta
curl -X POST http://localhost:5080/api/financeaccounts \
  -H "Content-Type: application/json" \
  -d '{"description":"Internet residencial","category":"Internet"}'

# 2. lançar (à vista, para mim mesmo)
curl -X POST http://localhost:5080/api/financeentries \
  -H "Content-Type: application/json" \
  -d '{
    "financeAccountId": "<id da conta>",
    "costCenterId": "<id do centro de custo Self>",
    "amount": 150.00,
    "dueDate": "2024-11-15",
    "installmentsCount": 1,
    "recurrenceType": "None",
    "isForecast": false
  }'
```

## Lembretes via rotinas (`schedule/`)

O lançamento não se vincula a nenhum agendamento. Quem agenda é uma **rotina** (`Routine`)
apontando para o módulo Financeiro, criada em "Rotinas" — ver
[`rotinas.md`](rotinas.md). A cada disparo, `schedule/Jobs/DateControlReminderJob.cs`:

1. Promove a `Overdue` os lançamentos `Pending` do módulo com vencimento no passado.
2. Avalia as regras de lembrete da rotina contra cada lançamento pendente/vencido:
   - **antes do vencimento** (`BeforeDueDays` dias antes, a partir de `BeforeDueTime`) —
     dispara uma vez por lançamento;
   - **no dia do vencimento** (`OnDueDate`) — uma vez por lançamento;
   - **repetir após vencer** (`AfterDueRepeatMinutes`) — a cada N minutos até ser paga.

   O "uma vez" é garantido por linhas em `DateControlReminderDispatches`.
3. Agrupa o que disparou em até 3 `Notification`s (uma por regra), numa `RoutineExecution`
   da rotina, e avisa a API via `pg_notify` (ver `notificacoes-realtime.md`).

"Hoje" é a data local da máquina que roda o worker.

## Decisões explícitas

- **Centro de custo fica no lançamento, não na conta.** Permite que a mesma conta
  cadastrada (ex: "Cartão de crédito") gere lançamentos para pessoas diferentes.
- **Sem repositório genérico** — ver `docs/ARCHITECTURE.md`.
- **UI não oferece mais "reaproveitar conta existente".** O modal "Novo lançamento"
  sempre cria uma `FinanceAccount` nova (decisão explícita do usuário: a aba "Conta
  existente" foi removida por ser redundante com a recorrência de quantidade fixa — pra
  repetir uma cobrança já existe a recorrência, não precisa reabrir a conta antiga toda
  vez). Consequência aceita: pode acabar com várias `FinanceAccount`s de mesma
  `Description` no banco; a API ainda permite reaproveitar (`POST /financeentries` com
  `financeAccountId` de uma conta já existente funciona normalmente), só a UI não expõe
  mais essa opção.

## Frontend (`frontend/src/pages/finance/`)

- **`Finance.tsx`** — tela principal em `/pessoal/financeiro`: 4 stat cards (total,
  pendentes, pagas, vencidas — com soma em R$), abas de filtro + busca por descrição da
  conta, e a tabela de lançamentos (ícone da categoria, valor, vencimento, "repetição"
  resumida — `describeRepetition()` em `financeLabels.ts` —, centro de custo, status,
  ações "Marcar como paga"/excluir).
- **`NewEntryModal.tsx`** — modal de criação. Sempre cria a `FinanceAccount` e, na
  sequência, o `FinanceEntry` (duas chamadas HTTP). Sem campo de parcelas — "repetir N
  vezes" só existe via Recorrência (quantidade fixa), pra não ter dois jeitos de dizer a
  mesma coisa. Os campos condicionais (quantidade de recorrência, datas de previsto) só
  aparecem quando fazem sentido, espelhando as regras do `FinanceEntry.EnsureValid()`.
- **`EditEntryModal.tsx`** — edição de um lançamento já criado: valor, vencimento,
  centro de custo, previsto. Não edita descrição/categoria/recorrência/parcelas (ver
  nota no `PUT` acima). Aberto pelo ícone de lápis na coluna Ações da tabela.
- Estilos de modal (`modal-overlay`/`modal-card*`/`field-row`/`checkbox-row`) viraram
  compartilhados em `src/components/Modal.css`, já que agora dois modais os usam.
- **`src/api/client.ts`** — `costCentersApi`, `financeAccountsApi`, `financeEntriesApi`
  (mesmo padrão do `employeesApi`). **`src/api/useFinanceData.ts`** — hook único que
  busca os três recursos em paralelo (contas, centros de custo, lançamentos) para
  alimentar a tela inteira.
- Os campos `.field`/`.field__label`/inputs do formulário foram promovidos de
  `CreateEmployee.css` para `theme.css` como utilitário global, já que agora são usados
  em duas features (Funcionários e Financeiro).
- Filtro de período (`periodFilter.ts`): select "Todos os períodos" agrupado por ano
  (`<optgroup>`), só com os meses/anos que realmente têm lançamentos.
- `<select>` do app inteiro (não só Financeiro) ganhou `appearance: none` + chevron
  próprio em `theme.css`, porque o controle nativo do SO (efeito "vidro") não combinava
  com o tema escuro.

Testado o fluxo completo via automação de navegador contra a API e o Postgres reais:
criar conta nova + lançamento com recorrência fixa/parcelado (série gerada corretamente,
numeração "2/3" etc.), editar valor/vencimento/centro de custo, listar, marcar como paga,
excluir — todos refletindo corretamente nos stat cards e contadores das abas, em claro e
escuro.

## Pendências

- [x] ~~Lembretes automáticos.~~ Via rotinas — ver
  [Lembretes via rotinas](#lembretes-via-rotinas-schedule) acima.
- [x] ~~Geração automática de parcelas/recorrências.~~ Implementado para `InstallmentsCount`
  e `RecurrenceType = FixedCount` — ver [Geração de séries](#geração-de-séries-parcelas-e-recorrência-fixa).
  `RecurrenceType = Indeterminate` continua pendente (depende do scheduler).
- [ ] **Série sem `SeriesId`.** As N linhas geradas por uma série são independentes — não
  dá pra excluir/editar "a série toda" de uma vez, só ocorrência por ocorrência.
- [ ] **Recorrência indeterminada sem geração.** O `schedule/` já existe e já roda
  diariamente, mas `FinanceReminderJob` só lembra — ainda não *gera* novas ocorrências de
  `RecurrenceType = Indeterminate`. Seria um novo job (ou uma extensão deste) que cria o
  próximo `FinanceEntry` da série quando o anterior é pago.
- [x] ~~Overdue automático.~~ `FinanceReminderJob` promove `Pending → Overdue` em lote a
  cada execução (ver seção acima) — a transição `Overdue → Paid` continua manual via
  `PATCH .../status`.
- [x] ~~Edição de `FinanceEntry`.~~ `PUT /api/financeentries/{id}` + `EditEntryModal.tsx`
  cobrem valor, vencimento, centro de custo e previsto. Descrição/categoria (pertencem à
  `FinanceAccount` compartilhada) e parcelas/recorrência (instrução de geração) ficam de
  fora de propósito — ver nota no endpoint acima.
- [ ] **CostCenter sem `PUT`.** Só criar/listar/excluir por enquanto.
- [ ] **FinanceAccount órfã acumulando.** Sem a opção de reaproveitar conta na UI, cada
  lançamento cria sua própria `FinanceAccount` — não tem deduplicação por descrição. Se
  isso virar um problema real de volume/poluição visual, dá pra reconsiderar.
- [ ] **Seed de centros de custo é só para dev.** `DbSeeder` roda automaticamente apenas
  quando `ASPNETCORE_ENVIRONMENT=Development`. Decidir o que acontece em produção (seed
  via migration com `HasData`? onboarding no primeiro uso do app?).
- [ ] **Paginação/filtros.** `GET /api/financeentries` retorna tudo; conforme o volume
  crescer vai precisar de filtro por conta, por centro de custo, por status, por período.
