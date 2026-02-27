# CRM Frontend

React + TypeScript SPA para consumir a API CRM (.NET).

## Stack

- **React 19** + **TypeScript**
- **Vite 7** (build tool e dev server)
- **React Router 7** (roteamento)
- CSS simples (sem dependência de UI library)

## Requisitos

- Node.js ≥ 18
- API CRM rodando (veja `src/CRM.Api`)

## Instalação

```bash
cd src/frontend
npm install
```

## Configuração

Copie o arquivo de exemplo e ajuste conforme necessário:

```bash
cp .env.example .env
```

| Variável             | Padrão                | Descrição                          |
|----------------------|-----------------------|------------------------------------|
| `VITE_API_BASE_URL`  | `""` (usa proxy Vite) | URL base da API .NET (opcional)    |

### Proxy Vite (recomendado para dev)

Por padrão, o dev server Vite faz proxy das rotas `/auth` e `/customers` para
`http://localhost:5178` (porta padrão do perfil `http` do `dotnet run`).
Você **não precisa configurar CORS** no backend ao usar este proxy.

Se a API rodar em outra porta, ajuste `VITE_API_BASE_URL` no `.env` **e** o
target do proxy no `vite.config.ts`:

```
VITE_API_BASE_URL=http://localhost:5178
```

Com `VITE_API_BASE_URL` definido, as requisições vão direto à API (sem proxy);
nesse caso é necessário habilitar CORS no backend para `http://localhost:5173`.

## Rodando em desenvolvimento

```bash
npm run dev
```

Acesse: <http://localhost:5173>

## Build de produção

```bash
npm run build
npm run preview
```

## Como fazer login

O endpoint de autenticação é demo-only (disponível apenas em `Development`):

- **Usuário:** `admin`
- **Senha:** `admin123`

O token JWT é armazenado no `localStorage` e injetado automaticamente em todas
as requisições protegidas via header `Authorization: Bearer <token>`.

## Rotas

| Rota                        | Descrição                               |
|-----------------------------|-----------------------------------------|
| `/login`                    | Tela de autenticação                    |
| `/customers`                | Listagem de clientes (busca + paginação)|
| `/customers/new`            | Formulário de cadastro (PF/PJ)          |
| `/customers/:id`            | Detalhes do cliente                     |
| `/customers/:id/events`     | Auditoria (timeline de eventos)         |

## Estrutura de pastas

```
src/
  context/        # AuthContext (estado do token JWT)
  components/     # Componentes reutilizáveis (PrivateRoute)
  pages/          # Páginas (Login, Customers, NewCustomer, Detail, Events)
  services/       # Clientes HTTP (api.ts, auth.ts, customers.ts)
  types/          # Interfaces TypeScript
```
