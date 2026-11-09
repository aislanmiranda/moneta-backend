# Contas e login

Cada pessoa tem a própria conta e só enxerga os próprios dados.

## Cadastro e login

- **Cadastro** (`POST /api/auth/register`): nome completo, e-mail (guardado para envios futuros,
  não serve para entrar), login (3–50 caracteres `a-z 0-9 . _ -`, único, sem diferença de
  maiúsculas) e senha de **exatamente 6 números**.
- **Login** (`POST /api/auth/login`) devolve um JWT válido por 30 dias (`Auth:TokenDays`). O
  frontend guarda no `localStorage` e manda em `Authorization: Bearer`; o SignalR recebe via
  `?access_token=` (aceito só em `/hubs`).
- **Bloqueio:** 5 senhas erradas seguidas bloqueiam a conta por 15 min — uma senha de 6 dígitos
  tem só 10⁶ combinações. A mensagem de erro é a mesma para login inexistente e senha errada.
- Senha guardada com o `PasswordHasher` do ASP.NET Identity (PBKDF2).
- **Chave de assinatura:** `Auth:JwtKey` (≥ 32 caracteres; env `Auth__JwtKey`). O valor em
  `appsettings.Development.json` é só para dev — em produção defina por variável de ambiente.

## Isolamento

Todo dado pertence a um `Module`, e todo `Module` tem `UserId`. O isolamento é aplicado em dois
pontos:

1. **Filtros globais do EF** (`AppDbContext.ConfigureTenancy`): módulos, lançamentos, contas,
   centros de custo, anexos, itens/ocorrências, agenda, rotinas, execuções e notificações só
   aparecem quando o módulo é do usuário atual (`ICurrentUser`). Vale para qualquer consulta,
   inclusive `GetById`, `ExecuteUpdate`/`ExecuteDelete` e `Include`.
2. **`ModuleOwnershipFilter`**: toda rota `/api/modules/{moduleId}/...` responde 404 se o módulo
   não for do usuário — cobre também as escritas.

Processos que atendem todos os usuários rodam como sistema: o `schedule/` (`SystemCurrentUser`)
e o repasse de notificações da API (`HttpCurrentUser.RunAsSystem()`), que entrega cada
notificação só nas conexões SignalR do dono (`Clients.User`).

## Contas novas e dados antigos

- A **primeira conta** criada herda todos os módulos que existiam antes das contas (com seus
  lançamentos, rotinas, notificações etc.).
- As contas seguintes (ou a primeira, num banco vazio) nascem com **Financeiro**, **Vida
  Saudável** e **Agenda** vazios, e um centro de custo "Eu" com o primeiro nome da pessoa.
