# Mundialito Tournament – Architecture

## High-level diagram

```
┌─────────────────────────────────────────────────────────────────────────────────┐
│                              DOCKER CONTAINERS                                    │
│  ┌──────────────┐  ┌──────────────┐  ┌──────────────────────────────────────┐   │
│  │   Frontend   │  │     API      │  │           SQL Server (db)             │   │
│  │  (Next.js)   │  │  (.NET 8)    │  │  Tables: Teams, Players, Matches,     │   │
│  │  port 3000   │  │  port 5000   │  │  MatchGoals, IdempotencyRecords       │   │
│  └──────┬───────┘  └──────┬───────┘  └──────────────────┬───────────────────┘   │
│         │                 │                             │                         │
│         │  HTTP           │  EF Core (writes)             │  Dapper (reads)          │
│         └────────────────┼──────────────────────────────┘                         │
└──────────────────────────┼────────────────────────────────────────────────────────┘
                          │
┌─────────────────────────┼─────────────────────────────────────────────────────────┐
│                    CLEAN ARCHITECTURE (API solution)                                │
│                         │                                                          │
│  ┌──────────────────────▼──────────────────────────────────────────────────────┐  │
│  │ API layer (MundialitoCorporativo.Api)                                        │  │
│  │ • Controllers (Teams, Players, Matches, Standings)                          │  │
│  │ • IdempotencyMiddleware (Idempotency-Key → IIdempotencyStore)               │  │
│  │ • Result → HTTP: 200, 201, 400, 404, 409                                     │  │
│  └──────────────────────┬──────────────────────────────────────────────────────┘  │
│                         │ MediatR Send(IRequest)                                    │
│  ┌──────────────────────▼──────────────────────────────────────────────────────┐  │
│  │ Application layer (CQRS)                                                      │  │
│  │ • Commands: CreateTeam, UpdateTeam, CreatePlayer, CreateMatch, SetResult…   │  │
│  │ • Queries: GetTeams, GetTeamById, GetPlayers, GetMatches, GetStandings…     │  │
│  │ • Handlers: use IAppDbContext (writes) / I*ReadRepository (reads)             │  │
│  │ • Result<T> (Success/Failure), PagedResult<T>, ErrorCodes                   │  │
│  └─────┬──────────────────────────────────────────────────────────────┬────────┘  │
│        │ Writes (EF)                                    Reads (Dapper) │            │
│  ┌─────▼─────────────────────────────────────┐  ┌───────▼──────────────────────┐  │
│  │ Infrastructure layer                       │  │ Same layer                    │  │
│  │ • AppDbContext (EF Core)                   │  │ • TeamReadRepository (Dapper)│  │
│  │ • Migrations, SeedData                     │  │ • PlayerReadRepository       │  │
│  │ • IdempotencyStore (EF IdempotencyRecords)  │  │ • MatchReadRepository        │  │
│  └─────┬─────────────────────────────────────┘  │ • StandingsReadRepository     │  │
│        │                                         └───────┬──────────────────────┘  │
│  ┌─────▼─────────────────────────────────────┐           │                         │
│  │ Domain layer                              │           │                         │
│  │ • Entities: Team, Player, Match, MatchGoal, IdempotencyRecord                   │
│  │ • Result<T>, MatchStatus enum              │           │                         │
│  └───────────────────────────────────────────┴───────────┴─────────────────────────┘  │
└──────────────────────────────────────────────────────────────────────────────────────┘
```

## CQRS flow

- **Write path:** Controller → MediatR → Command Handler → `IAppDbContext` (EF Core) → SQL Server.  
  No exceptions for business failures; use `Result.Failure` and map to 400/404/409 in the API.

- **Read path:** Controller → MediatR → Query Handler → `I*ReadRepository` (Dapper) → SQL Server.  
  All list endpoints use database-level pagination (`OFFSET/FETCH`), filters, and sorting.

## Idempotency

- **POST** requests may send header `Idempotency-Key: <key>`.
- `IdempotencyMiddleware` checks `IIdempotencyStore` (key + method + path). If a previous response exists, it returns the same status and body without running the handler again.
- Stored in table `IdempotencyRecords` (EF) with key, method, path, status code, and response body.

## HTTP status mapping (Result pattern)

No se usan excepciones para el control de flujo de negocio. Los handlers devuelven `Result<T>.Success(data)` o `Result<T>.Failure(message, errorCode)`; la API mapea el código a HTTP. Detalle: [RESULT_AND_HTTP.md](./RESULT_AND_HTTP.md).

| Result              | HTTP   |
|---------------------|--------|
| Success + data      | 200 OK |
| Success + created   | 201 Created |
| Failure + NotFound  | 404 Not Found |
| Failure + Conflict/Duplicate | 409 Conflict |
| Failure + Validation/other   | 400 Bad Request |
| Delete success      | 204 No Content |

## API versioning

- All routes are under **`api/v1/`** (e.g. `api/v1/teams`, `api/v1/standings/top-scorers`).
- Implemented with **Asp.Versioning.Mvc**; default version 1.0, URL segment versioning. Future versions (e.g. v2) can coexist.

## Pagination

- Query params: `pageNumber`, `pageSize`, `sortBy`, `sortDirection`, plus entity-specific filters.
- Response shape: `{ "data": [], "pageNumber": 1, "pageSize": 10, "totalRecords": 50, "totalPages": 5 }`.

## Standings

- Order: **Points** (DESC) → **Goal differential** (DESC) → **Goals for** (DESC).
- Points: Win = 3, Draw = 1, Loss = 0.
- Implemented in `StandingsReadRepository` via a single Dapper query (CTE over completed matches).
