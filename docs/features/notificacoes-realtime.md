# Feature: Notificações em tempo real

**Status:** implementado ponta a ponta — worker grava a notificação, backend relay via
SignalR, frontend (PWA) exibe notificação do dispositivo + sino/dropdown no topbar + página
`/configuracoes/notificacoes` com o histórico completo.

## Por que este desenho

Os jobs das rotinas (ex: `DateControlReminderJob`, ver
[financeiro-pessoal.md](financeiro-pessoal.md)) rodam no `schedule/`, um processo
separado da API, e já gravam `Notification` direto no Postgres compartilhado — não fazem
chamada HTTP pra API (ver decisão de desacoplamento documentada no `schedule/README.md`).
O desafio era levar esse evento até o frontend **sem** quebrar esse desacoplamento, ou
seja, sem o worker precisar saber se a API está de pé, nem a API precisar fazer polling
constante no banco.

A solução: **Postgres LISTEN/NOTIFY** como ponte. O worker grava a notificação e manda um
aviso leve (`pg_notify`); a API mantém uma escuta permanente nesse canal e, ao receber o
aviso, busca os detalhes e repassa via **SignalR** pra quem estiver conectado. Nenhum dos
dois processos precisa saber da existência do outro — só compartilham o banco, como já
acontecia.

## Fluxo

```
schedule/DateControlReminderJob (disparado por uma rotina)
  → grava Notification no Postgres
  → SELECT pg_notify('notifications_channel', '<id>')
       ↓ (Postgres entrega a todo LISTEN ativo no canal)
backend/Moneta.Api — PostgresNotificationListener (BackgroundService)
  → recebe o payload (id da notificação)
  → busca a notificação completa via INotificationService
  → repassa para o hub SignalR
       ↓
backend/Moneta.Api — NotificationsHub (/hubs/notifications)
  → Clients.All.SendAsync("ReceiveNotification", dto)
       ↓
frontend — NotificationsProvider (SignalR client)
  → atualiza o estado em memória (sino + página de histórico)
  → agrupa a rajada por módulo (debounce de 2s) e mostra uma notificação do dispositivo
     via service worker (registration.showNotification); tocar nela abre o módulo
```

## Backend

- **`Domain/Entities/Notification.cs`** já existia (`Message`, `DeliveredAt`, `ReadAt`,
  `ExecutionId`) — nada mudou aqui, só passou a ser exposto pela API.
- **`Application/Notifications/`** + **`Repository/Notifications/`** — serviço/repositório
  no mesmo padrão dos demais recursos (`GetAllAsync` ordenado por `DeliveredAt` desc,
  `MarkAsReadAsync`).
- **`Api/Controllers/NotificationsController.cs`**:
  ```
  GET   /api/notifications
  PATCH /api/notifications/{id}/read
  ```
- **`Api/Realtime/NotificationsHub.cs`** — hub SignalR vazio de propósito: é só um canal
  de broadcast, o cliente nunca chama método nenhum nele, só escuta
  `"ReceiveNotification"`.
- **`Api/Realtime/PostgresNotificationListener.cs`** — `BackgroundService` com uma
  conexão Npgsql dedicada (`LISTEN notifications_channel;` + `connection.WaitAsync(...)`
  em loop). Se a conexão cair (Postgres reiniciou, rede), reconecta sozinho após 5s —
  loop externo com `try/catch` em volta do loop de escuta.
- **Mesma origem**: o frontend chama `/api` e `/hubs` na própria origem — o Vite (dev) e o
  nginx do container `frontend` (produção) fazem proxy pro backend, incluindo o upgrade de
  WebSocket do SignalR. A policy CORS `"Frontend"` (com `.AllowCredentials()`, exigido pelo
  handshake do SignalR) só importa quando o app aponta direto pra API via `VITE_API_ORIGIN`.

### Por que `pg_notify` e não uma tabela de outbox ou fila

Dado o volume baixo esperado (lembretes pessoais, não eventos de alto throughput) e que
`schedule/` e a API já compartilham o mesmo Postgres, `pg_notify`/`LISTEN` evita
infraestrutura extra (fila, outbox table, poller). Limitação aceita: notificações
emitidas **enquanto a API está fora do ar** se perdem no canal (Postgres não as guarda) —
mas a notificação em si já está persistida na tabela `Notifications`, então
`GET /api/notifications` sempre reflete a realidade quando o frontend abre/reconecta; só
o *toast em tempo real* daquele evento específico é perdido, não o dado.

## Frontend

- **`notifications/NotificationsProvider.tsx`** — contexto React que:
  1. Busca o histórico inicial (`GET /api/notifications`) ao montar.
  2. Abre uma conexão SignalR (`@microsoft/signalr`) pra `/hubs/notifications`, com
     reconexão automática; se a conexão inicial falhar (API fora do ar), tenta de novo a
     cada 5s.
  3. Ao receber `"ReceiveNotification"`, atualiza o estado local e, se o usuário liberou
     notificações, mostra uma por módulo via `registration.showNotification(...)` com
     `data.url` = rota do módulo.
  4. Escuta mensagens do service worker (`src/sw.ts`): ao tocar numa notificação, o SW foca
     a janela aberta e pede pra navegar até `data.url` (ou abre uma janela nova).
  5. Expõe `devicePermission`/`requestDevicePermission` — a permissão é pedida por um botão
     em `/configuracoes/notificacoes` (iOS só aceita o pedido a partir de um toque).
- **`components/NotificationBell.tsx`** — sino no topbar (`layout/AppShell.tsx`) com
  contador de não lidas e dropdown das 8 mais recentes.
- **`pages/notifications/NotificationsPage.tsx`** — histórico completo em
  `/configuracoes/notificacoes` (rota que antes era só um placeholder "Coming Soon").
- **PWA**: `vite-plugin-pwa` com service worker próprio (`src/sw.ts`, estratégia
  `injectManifest`) — precache do app shell e tratamento de `notificationclick`.

## O que não foi (ainda) validado

O pipeline **worker → Postgres NOTIFY → API → SignalR → cliente** foi validado
ponta a ponta com um cliente de teste (`@microsoft/signalr` via Node), incluindo
`GET`/`PATCH` de notificações. As notificações do dispositivo (service worker + permissão) precisam ser validadas
manualmente no celular, com o app instalado e servido por HTTPS.

**Limitação:** as notificações só chegam enquanto o app está aberto (a conexão SignalR vive
na página). Para receber lembretes com o app fechado é preciso Web Push (VAPID + inscrição
do dispositivo + envio pelo `schedule/`) — ainda não implementado.

## Pendências

- [ ] **Perda de eventos com a API fora do ar.** `pg_notify` não enfileira — se nenhum
  `LISTEN` estiver ativo no momento, o evento se perde (o dado na tabela não, só o push
  em tempo real). Aceitável no uso atual; se virar problema, dá pra adicionar um
  outbox/replay (ex: frontend busca `GET /api/notifications?unreadOnly=true` ao
  reconectar, cobrindo a lacuna).
- [ ] **Sem paginação em `GET /api/notifications`.** Mesma observação já feita para
  `FinanceEntry` — se o volume crescer, precisa de filtro/paginação.
- [ ] **Validação manual das notificações no celular** (ver seção acima).
- [ ] **Web Push** para notificar com o app fechado.
