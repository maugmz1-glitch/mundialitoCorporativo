# Architecture diagram (Mermaid)

```mermaid
flowchart TB
  subgraph Docker["Docker containers"]
    FE[Frontend Next.js :3000]
    API[API .NET :5000]
    DB[(SQL Server :1433)]
  end

  subgraph API_Layer["API layer"]
    C[Controllers]
    IDEM[Idempotency Middleware]
  end

  subgraph App_Layer["Application layer (CQRS)"]
    CMD[Commands + Handlers]
    Q[Queries + Handlers]
  end

  subgraph Infra["Infrastructure layer"]
    EFC[EF Core DbContext]
    DR[Dapper Read Repos]
    IDEMSTORE[Idempotency Store]
  end

  subgraph Domain["Domain layer"]
    E[Entities]
    R[Result pattern]
  end

  FE -->|HTTP| API
  API --> IDEM
  IDEM --> C
  C -->|MediatR| CMD
  C -->|MediatR| Q
  CMD --> EFC
  CMD --> IDEMSTORE
  Q --> DR
  EFC --> DB
  DR --> DB
  IDEMSTORE --> EFC
  EFC --> E
  CMD --> R
  Q --> R
```

## CQRS and data flow

- **Write path:** Controller → Command Handler → IAppDbContext (EF Core) → Database.
- **Read path:** Controller → Query Handler → I*ReadRepository (Dapper) → Database.
- **Idempotency:** POST with Idempotency-Key → Middleware → IIdempotencyStore (EF) → return stored response or run handler and store.
- **Result pattern:** Handlers return Result&lt;T&gt;; API maps to 200, 201, 400, 404, 409.
