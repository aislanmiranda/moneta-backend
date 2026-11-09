# Configuração de módulos

Botão **Configurar** no cabeçalho do Financeiro e do Vida Saudável abre um modal com duas abas:

| Módulo (padrão) | Aba 1 | Aba 2 |
|---|---|---|
| Financeiro (`DateControl`) | Centros de custo (`DateControlParty`) | Categorias (`DateControlCategory`) |
| Vida Saudável (`TimeControl`) | Pessoas (`TimeControlParty`) | Tipos (`TimeControlType`) |

Em cada aba: adicionar, editar (nome + relação, ou nome + ícone) e excluir. Regras, iguais
para as quatro listas:

- Nome obrigatório (até 100), **único por módulo** sem diferenciar maiúsculas.
- **Excluir é bloqueado enquanto estiver em uso** — a API responde 400 com "está em uso em N
  lançamento(s)/conta(s)/item(ns)" e a tela já desabilita a lixeira (cada item traz `usageCount`).
- Tudo é por módulo e por usuário (filtros globais; ver `contas-e-login.md`). Uma categoria ou
  tipo de outro módulo é recusado (404) ao criar/editar conta ou item.

## Categorias e tipos editáveis

Antes eram enums fixos (`DateControlSourceCategory`, `TimeControlItemType`). Agora são tabelas
por módulo com `Name` e `Icon` (nome de um ícone Lucide; o frontend oferece a lista em
`frontend/src/constants/entityIcons.ts` e cai num ícone genérico para nomes desconhecidos).

- Módulos novos (cadastro de conta ou "Criar módulo") nascem com a lista padrão
  (`ModuleDefaults`): Internet, Energia, Água, Cartão, Assinatura, Aluguel, Outro, Saúde, Lazer /
  Remédio, Água, Academia.
- A migration `EditableCategoriesAndTypes` criou a lista padrão em cada módulo existente e
  apontou cada conta/item para o equivalente do valor antigo.
- O lembrete do Vida Saudável (`TimeControlReminderJob`) agrupa pelo tipo cadastrado:
  "2 pendentes em Remédio: Vitamina D, Omeprazol".
- O campo "Detalhe / dosagem" do item vale para qualquer tipo (antes só para Remédio).

## API

```
GET|POST        /api/modules/{moduleId}/parties          (centros de custo)
PUT|DELETE      /api/modules/{moduleId}/parties/{id}
GET|POST        /api/modules/{moduleId}/categories       { name, icon }
PUT|DELETE      /api/modules/{moduleId}/categories/{id}
GET|POST        /api/modules/{moduleId}/time-parties     (pessoas)
PUT|DELETE      /api/modules/{moduleId}/time-parties/{id}
GET|POST        /api/modules/{moduleId}/time-types       { name, icon }
PUT|DELETE      /api/modules/{moduleId}/time-types/{id}
```
