# CRM System - Customer Relationship Management

Sistema completo de CRM (Customer Relationship Management) construído com arquitetura moderna, seguindo princípios de Clean Architecture, CQRS e Event Sourcing.

## 📋 Índice

- [Visão Geral](#visão-geral)
- [Arquitetura](#arquitetura)
- [Stack Tecnológica](#stack-tecnológica)
- [Pré-requisitos](#pré-requisitos)
- [Como Executar](#como-executar)
- [Estrutura do Projeto](#estrutura-do-projeto)
- [ADRs - Decisões Arquiteturais](#adrs---decisões-arquiteturais)
- [Endpoints da API](#endpoints-da-api)
- [Testes](#testes)

---

## 🎯 Visão Geral

Sistema de gerenciamento de clientes (CRM) que oferece:

- ✅ Cadastro de clientes Pessoa Física (PF) e Pessoa Jurídica (PJ)
- ✅ Histórico completo de alterações via Event Sourcing
- ✅ Autenticação JWT
- ✅ Integração com ViaCEP para autocompletar endereços
- ✅ Frontend React com TypeScript
- ✅ Containerização completa com Docker

---

## 🏗 Arquitetura

O sistema segue **Clean Architecture** com separação clara de responsabilidades:

```
┌─────────────────────────────────────────────────────────────────┐
│                        FRONTEND (React)                         │
│                   SPA com React 19 + TypeScript                 │
└─────────────────────────────────────────────────────────────────┘
                                  │
                                  ▼
┌─────────────────────────────────────────────────────────────────┐
│                          API (.NET 10)                          │
│  ┌─────────────┐  ┌─────────────┐  ┌─────────────────────────┐ │
│  │  Endpoints  │──│   MediatR   │──│     Command Handlers    │ │
│  │  (Minimal)  │  │   (CQRS)    │  │     Query Handlers      │ │
│  └─────────────┘  └─────────────┘  └─────────────────────────┘ │
└─────────────────────────────────────────────────────────────────┘
                                  │
          ┌───────────────────────┼───────────────────────┐
          ▼                       ▼                       ▼
┌─────────────────┐    ┌─────────────────┐    ┌─────────────────┐
│   Event Store   │    │   Read Model    │    │    ViaCEP API   │
│   (Append-Only) │    │   (Projection)  │    │   (Endereços)   │
└─────────────────┘    └─────────────────┘    └─────────────────┘
          │                       │
          └───────────┬───────────┘
                      ▼
          ┌─────────────────────┐
          │    PostgreSQL 16    │
          │  ┌───────────────┐  │
          │  │  event_store  │  │  ← Eventos imutáveis
          │  ├───────────────┤  │
          │  │customers_read │  │  ← Projeção para leitura
          │  └───────────────┘  │
          └─────────────────────┘
```

### Padrões Implementados

| Padrão | Descrição |
|--------|-----------|
| **CQRS** | Separação entre escrita (Commands) e leitura (Queries) |
| **Event Sourcing** | Estado derivado de sequência de eventos imutáveis |
| **Mediator** | Desacoplamento via MediatR |
| **Repository** | Abstração de persistência |
| **Projection** | Materialização de read models a partir de eventos |
| **Domain-Driven Design** | Aggregates, Value Objects, Domain Events |

---

## 🛠 Stack Tecnológica

### Backend
| Tecnologia | Versão | Propósito |
|------------|--------|-----------|
| .NET | 10.0 | Framework principal |
| PostgreSQL | 16 | Banco de dados |
| Dapper | - | Micro ORM (queries) |
| MediatR | - | CQRS/Mediator |
| Serilog | - | Logging estruturado |
| Polly | - | Resiliência (retry, timeout) |
| JWT Bearer | - | Autenticação |

### Frontend
| Tecnologia | Versão | Propósito |
|------------|--------|-----------|
| React | 19.2 | UI Framework |
| TypeScript | 5.9 | Type safety |
| Vite | 7.3 | Build tool |
| React Router | 7.13 | Navegação |

### Infraestrutura
| Tecnologia | Propósito |
|------------|-----------|
| Docker | Containerização |
| Docker Compose | Orquestração local |

---

## 📦 Pré-requisitos

- [Docker](https://www.docker.com/get-started) e Docker Compose
- OU
  - [.NET 10 SDK](https://dotnet.microsoft.com/download/dotnet/10.0)
  - [Node.js 20+](https://nodejs.org/)
  - [PostgreSQL 16+](https://www.postgresql.org/download/)

---

## 🚀 Como Executar

### Opção 1: Docker Compose (Recomendado)

```bash
# Clone o repositório
git clone https://github.com/gitpagimaxx/crm-source.git
cd crm-source

# Inicie todos os serviços
docker-compose up -d

# Verifique os logs
docker-compose logs -f
```

**Serviços disponíveis:**
| Serviço | URL | Descrição |
|---------|-----|-----------|
| Frontend | http://localhost:5173 | Aplicação React |
| API | http://localhost:7148 | Backend .NET |
| Swagger | http://localhost:7148/swagger | Documentação API |
| PostgreSQL | localhost:5433 | Banco de dados |

### Opção 2: Execução Manual

#### 1. Banco de Dados
```bash
# Via Docker
docker run -d \
  --name crm_postgres \
  -e POSTGRES_USER=postgres \
  -e POSTGRES_PASSWORD=postgres \
  -e POSTGRES_DB=crm \
  -p 5433:5432 \
  -v ./sql:/docker-entrypoint-initdb.d:ro \
  postgres:16
```

#### 2. Backend
```bash
cd api
dotnet restore
dotnet run --project src/CRM.Backend.Api
```

#### 3. Frontend
```bash
cd frontend
npm install
npm run dev
```

---

## 📁 Estrutura do Projeto

```
crm-source/
├── api/                              # Backend .NET
│   ├── src/
│   │   ├── CRM.Backend.Api/          # Camada de apresentação (endpoints, middleware)
│   │   ├── CRM.Backend.Application/  # Casos de uso (commands, queries, handlers)
│   │   ├── CRM.Backend.Domain/       # Domínio (aggregates, eventos, value objects)
│   │   └── CRM.Backend.Infra/        # Infraestrutura (repositórios, serviços externos)
│   └── tests/
│       ├── CRM.Backend.Tests.Unit/       # Testes unitários
│       └── CRM.Backend.Tests.Integration # Testes de integração
├── frontend/                         # Frontend React
│   └── src/
│       ├── components/               # Componentes reutilizáveis
│       ├── context/                  # Context API (auth)
│       ├── pages/                    # Páginas da aplicação
│       ├── services/                 # Chamadas à API
│       └── types/                    # TypeScript types
├── sql/                              # Scripts de inicialização do banco
│   ├── 001_event_store.sql           # Tabela de eventos
│   └── 002_customers_read.sql        # Read model de clientes
└── docker-compose.yml                # Orquestração dos serviços
```

---

## 📐 ADRs - Decisões Arquiteturais

### ADR-001: PostgreSQL como Banco de Dados

**Contexto:**  
Precisávamos de um banco de dados que suportasse tanto o armazenamento de eventos (JSON) quanto queries relacionais eficientes para o read model.

**Decisão:**  
Escolhemos PostgreSQL 16.

**Justificativas:**
- **JSONB nativo**: Suporte eficiente para armazenar eventos como JSON com indexação
- **ACID compliant**: Garantia de consistência para o event store append-only
- **Maturidade**: Banco estável, amplamente documentado e com excelente comunidade
- **Performance**: Excelente para operações de leitura com índices otimizados
- **Custo**: Open source, sem custos de licenciamento
- **Ecossistema .NET**: Suporte completo via Npgsql e Dapper

**Alternativas consideradas:**
- *MongoDB*: Bom para eventos, mas perderia consistência transacional
- *EventStoreDB*: Especializado em event sourcing, porém adiciona complexidade operacional
- *SQL Server*: Bom suporte, mas custo de licenciamento em produção

---

### ADR-002: Event Sourcing para Persistência

**Contexto:**  
O CRM precisa de auditoria completa de todas as alterações em clientes, com capacidade de reconstruir o histórico.

**Decisão:**  
Implementamos Event Sourcing com tabela `event_store` própria.

**Estrutura do Event Store:**

```sql
CREATE TABLE event_store (
    id BIGSERIAL PRIMARY KEY,
    stream_id UUID NOT NULL,           -- ID do aggregate (cliente)
    event_type VARCHAR(200) NOT NULL,  -- Tipo do evento (ex: CustomerCreatedEvent)
    event_data JSONB NOT NULL,         -- Payload do evento serializado
    metadata JSONB NOT NULL,           -- Metadados (actor, correlation_id)
    stream_version INT NOT NULL,       -- Versão para controle de concorrência
    created_at TIMESTAMPTZ NOT NULL,   -- Timestamp do evento
    actor_user_id VARCHAR(200),        -- Quem executou a ação
    actor_email VARCHAR(200),
    actor_name VARCHAR(200),
    correlation_id VARCHAR(200)        -- Rastreabilidade distribuída
);
```

**Justificativas:**
- **Auditoria completa**: Histórico imutável de todas as operações
- **Reconstrução de estado**: Possibilidade de "viajar no tempo"
- **Debug facilitado**: Eventos explicitam o que aconteceu
- **Projections flexíveis**: Read models podem ser recriados a qualquer momento
- **Integração**: Eventos podem ser publicados para outros sistemas

**Trade-offs:**
- Complexidade adicional vs CRUD tradicional
- Necessidade de projeções para leitura
- Eventual consistency entre write e read

---

### ADR-003: CQRS (Command Query Responsibility Segregation)

**Contexto:**  
Com Event Sourcing, as leituras a partir de eventos reconstruídos seriam lentas. Precisávamos separar escrita e leitura.

**Decisão:**  
Implementamos CQRS usando MediatR.

**Fluxo de Escrita (Command):**
```
Request → Command → Handler → Aggregate → Event → EventStore → Projection → ReadModel
```

**Fluxo de Leitura (Query):**
```
Request → Query → Handler → ReadRepository → ReadModel → Response
```

**Justificativas:**
- **Performance**: Leituras otimizadas com Dapper direto no read model
- **Escalabilidade**: Read e write podem escalar independentemente
- **Modelagem livre**: Read model formatado para a UI, não para o domínio
- **MediatR**: Desacoplamento entre endpoints e lógica de negócio

---

### ADR-004: Projections Síncronas

**Contexto:**  
Decidir se as projeções (atualização do read model) seriam síncronas ou assíncronas.

**Decisão:**  
Projections são executadas de forma síncrona após a escrita no event store.

```csharp
// Após commit dos eventos
foreach (var projection in _projections)
    await projection.ProjectAsync(evt, ct);
```

**Justificativas:**
- **Simplicidade**: Sem necessidade de message broker
- **Consistência**: Leitura imediata após escrita
- **Adequado ao volume**: Para o cenário atual de CRM, é suficiente

**Trade-offs:**
- Latência adicional nas escritas
- Se projeção falhar, o evento já foi persistido (logs de warning)

**Evolução futura:**  
Para maior escala, migrar para projections assíncronas via RabbitMQ/Kafka.

---

### ADR-005: Dapper vs Entity Framework

**Contexto:**  
Escolha do ORM/data access para queries de leitura.

**Decisão:**  
Usamos Dapper para leituras e comandos SQL diretos para escritas.

**Justificativas:**
- **Performance**: Dapper é mais rápido que EF para queries de leitura
- **Controle**: SQL explícito, sem "magia" do ORM
- **Event Sourcing**: Não precisamos de change tracking
- **Simplicidade**: Read models são simples, não precisam de mapeamento complexo

**Alternativas:**
- *Entity Framework*: Overhead desnecessário para nosso caso
- *ADO.NET puro*: Muito verboso

---

### ADR-006: Autenticação JWT

**Contexto:**  
API REST precisa de autenticação stateless.

**Decisão:**  
JWT Bearer tokens com chave simétrica.

**Configuração:**
```json
{
  "Jwt": {
    "Key": "chave-secreta-64-chars-minimo",
    "Issuer": "CRM.Api",
    "Audience": "CRM.Clients"
  }
}
```

**Justificativas:**
- **Stateless**: Não requer sessão no servidor
- **Padrão**: Amplamente suportado
- **Claims**: Informações do usuário no token (userId, email, name)
- **UserContext**: Claims extraídos e disponíveis via DI para auditoria

---

### ADR-007: Clean Architecture em Camadas

**Contexto:**  
Organização do código para manutenibilidade e testabilidade.

**Decisão:**  
4 camadas seguindo Clean Architecture:

```
CRM.Backend.Api           ← Presentation (endpoints, middleware)
    ↓
CRM.Backend.Application   ← Use Cases (commands, queries, handlers)
    ↓
CRM.Backend.Domain        ← Business Rules (aggregates, events, value objects)
    ↓
CRM.Backend.Infra         ← External Concerns (DB, APIs externas)
```

**Justificativas:**
- **Testabilidade**: Domínio isolado, fácil de testar
- **Independência**: Domínio não conhece infraestrutura
- **Substituibilidade**: Trocar banco/framework sem afetar regras de negócio

---

### ADR-008: Integração ViaCEP com Resiliência

**Contexto:**  
Autocompletar endereços a partir do CEP, API externa pode falhar.

**Decisão:**  
Integração via HttpClient com tratamento de falhas.

```csharp
public async Task<ViaCepResult?> GetAddress(string zipCode, CancellationToken ct)
{
    try {
        var response = await _httpClient.GetStringAsync($"https://viacep.com.br/ws/{cleaned}/json/", ct);
        // ...
    } catch (Exception ex) {
        _logger.LogWarning(ex, "Failed to fetch address from ViaCEP");
        return null; // Graceful degradation
    }
}
```

**Justificativas:**
- **UX**: Facilita preenchimento de endereço
- **Resiliência**: Fallback se API estiver indisponível
- **Polly**: Retry policies podem ser adicionadas

---

### ADR-009: Value Objects para Validação

**Contexto:**  
Validação de CPF/CNPJ e outros dados de domínio.

**Decisão:**  
Value Objects que encapsulam validação.

```csharp
public record Document
{
    public string Value { get; }
    
    public Document(string value, CustomerType type)
    {
        // Valida CPF ou CNPJ conforme tipo
        Value = type == CustomerType.PF 
            ? ValidateCpf(value) 
            : ValidateCnpj(value);
    }
}
```

**Justificativas:**
- **Invariantes garantidas**: Documento inválido não é criado
- **Reutilização**: Validação em um só lugar
- **Domain-Driven Design**: Conceitos do domínio explícitos

---

## 📡 Endpoints da API

### Autenticação
| Método | Endpoint | Descrição |
|--------|----------|-----------|
| POST | `/auth/login` | Login (retorna JWT) |

### Clientes
| Método | Endpoint | Descrição | Auth |
|--------|----------|-----------|------|
| GET | `/customers` | Lista clientes (paginado) | ✅ |
| GET | `/customers/{id}` | Detalhe do cliente | ✅ |
| GET | `/customers/{id}/events` | Histórico de eventos | ✅ |
| POST | `/customers` | Criar cliente | ✅ |
| PUT | `/customers/{id}` | Atualizar cliente | ✅ |
| DELETE | `/customers/{id}` | Desativar cliente | ✅ |

### Utilitários
| Método | Endpoint | Descrição | Auth |
|--------|----------|-----------|------|
| GET | `/address/{cep}` | Consulta endereço via CEP | ✅ |

---

## 🧪 Testes

### Executar Testes Unitários
```bash
cd api
dotnet test tests/CRM.Backend.Tests.Unit
```

### Executar Testes de Integração
```bash
# Requer PostgreSQL rodando
cd api
dotnet test tests/CRM.Backend.Tests.Integration
```

---

## 📄 Licença

Este projeto é distribuído sob licença MIT.