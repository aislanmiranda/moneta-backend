# Rotinas

Uma **rotina** é um agendamento criado pelo usuário: "verifique este módulo nesta frequência e
me avise". Substitui o antigo modelo de funcionários/atividades (`Employee`/`AgentTask`) — não
há mais "quem executa", só o quê e quando.

## Modelo

`Routine` (`Domain/Entities/Routine.cs`), uma linha por rotina, com o agendamento achatado na
própria linha (mesma forma de `TimeControlItem`):

| Campo | Para quê |
|---|---|
| `Title` | Nome exibido na lista e nos logs do scheduler. |
| `ModuleId` | Qual módulo a rotina monitora. Fixo depois de criada. |
| `Type` | `RoutineType`, derivado do `Pattern` do módulo na criação (`DateControl → DateControlReminder`, etc.). Decide qual job executa. |
| `Payload` | JSON específico do tipo — hoje só as regras de lembrete de `DateControlReminder` (`ReminderRulesDto`). |
| `Status` | `Active` (tem trigger no Quartz) ou `Paused`. |
| `CronExpression`, `DaysOfWeek`, `Hours`, `Minute` | Agendamento — "modo simples" (dias + horas + minuto, composto por `CronBuilder`) ou "modo avançado" (cron digitado, `Hours = null`). |
| `NextRunAt` | Escrito pelo `ScheduleSyncJob` a partir do trigger real. |
| `TriggerRequestedAt` | Pedido de "Executar agora", consumido pelo `ScheduleSyncJob`. |

Cada disparo grava uma `RoutineExecution`; as `Notification`s geradas apontam para ela.

## API

| Método | Rota | |
|---|---|---|
| `GET` | `/api/routines` | Lista todas (com nome do módulo e última execução). |
| `GET` | `/api/routines/{id}` | |
| `POST` | `/api/routines` | `{ title, moduleId, schedule, reminderRules }` — já nasce `Active` e com `TriggerRequestedAt` preenchido (roda uma vez em seguida). |
| `PUT` | `/api/routines/{id}` | `{ title, schedule, reminderRules }` — módulo não muda. |
| `PATCH` | `/api/routines/{id}/status` | `{ status: "Active" \| "Paused" }` |
| `POST` | `/api/routines/{id}/trigger-now` | "Executar agora". |
| `DELETE` | `/api/routines/{id}` | Remove rotina, execuções e notificações delas (cascade). |

## Frontend

Página `/rotinas` (`frontend/src/pages/routines/`): tabela (vira lista de cartões no celular)
com pausar/retomar, executar agora, editar e excluir; `RoutineModal` cria e edita, reaproveitando
`components/schedule/ScheduleField`.

## Migração dos dados antigos

A migration `RemoveAgentsAddRoutines` copia cada `AgentTask` (com seu `TaskSchedule`) para
`Routines` mantendo o mesmo id, renomeia as execuções para `RoutineExecutions` mantendo os
vínculos das notificações, e remove `Employees`, `AgentTasks`, `TaskSchedules`,
`TaskExecutions`, `Capabilities`, `EmployeeCapabilities`, `ToolConnections` e `AiProviders`.
`AgentTaskStatus.Active` vira `RoutineStatus.Active`; qualquer outro status vira `Paused`.
