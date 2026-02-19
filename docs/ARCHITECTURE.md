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

| Result              | HTTP   |
|---------------------|--------|
| Success + data      | 200 OK |
| Success + created   | 201 Created |
| Failure + NotFound  | 404 Not Found |
| Failure + Conflict/Duplicate | 409 Conflict |
| Failure + Validation/other   | 400 Bad Request |
| Delete success      | 204 No Content |

## Pagination

- Query params: `pageNumber`, `pageSize`, `sortBy`, `sortDirection`, plus entity-specific filters.
- Response shape: `{ "data": [], "pageNumber": 1, "pageSize": 10, "totalRecords": 50, "totalPages": 5 }`.

## Standings

- Order: **Points** (DESC) → **Goal differential** (DESC) → **Goals for** (DESC).
- Points: Win = 3, Draw = 1, Loss = 0.
- Implemented in `StandingsReadRepository` via a single Dapper query (CTE over completed matches).
