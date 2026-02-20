# Mundialito Tournament Management System

Enterprise-style tournament management with Clean Architecture, CQRS, EF Core (writes), Dapper (reads), Result pattern, idempotency, and a Next.js frontend.

## Subir a GitHub

El repo incluye las ramas **main**, **development** y **release**. Pasos: crear un repositorio nuevo en GitHub (sin README/.gitignore), luego en la raíz del proyecto:

```powershell
git remote add origin https://github.com/TU_USUARIO/TU_REPO.git
git push -u origin main
git push -u origin development
git push -u origin release
```

O usa el script: `.\scripts\push-to-github.ps1 -GitHubUrl "https://github.com/TU_USUARIO/TU_REPO.git"`.  
Guía completa: **docs/SUBIR_A_GITHUB.md**.

## Features

- **Teams** – CRUD, list with filters, pagination, sorting
- **Players** – Per-team registration, filters, pagination
- **Matches** – Create, update, set result (PATCH), filter by date/team/status
- **Standings** – Auto-calculated table (points, goal differential, goals for) and top scorers
- **Idempotency** – POST with `Idempotency-Key` returns same response on retry
- **REST** – 200, 201, 204, 400, 404, 409; paginated response shape

## Solution structure

- **src/MundialitoCorporativo.Domain** – Entities, Result pattern
- **src/MundialitoCorporativo.Application** – CQRS (commands/queries/handlers), DTOs, interfaces
- **src/MundialitoCorporativo.Infrastructure** – EF Core DbContext, migrations, Dapper read repos, idempotency store
- **src/MundialitoCorporativo.Api** – Controllers, middleware, HTTP mapping
- **tests/MundialitoCorporativo.Tests** – Unit tests (standings logic, idempotency)
- **frontend** – Next.js (React, TypeScript) for teams, players, matches, standings
- **postman** – Postman collection (endpoints, idempotency, filters, pagination, errors)
- **docs** – ARCHITECTURE.md, GIT_WORKFLOW.md

## Prerequisites

- .NET 8 SDK
- Node 20+ (for frontend)
- SQL Server (local or Docker)
- Docker & Docker Compose (optional, for full stack)

**Si el IDE muestra "Unable to retrieve project metadata. Ensure it's an SDK-style project":**
- Abre la **carpeta raíz del repo** (donde está `MundialitoCorporativo.sln`), no una subcarpeta como `frontend`.
- En Cursor/VS Code: **File → Open Folder** y elige la carpeta que contiene el `.sln`.
- Los comandos `dotnet` y `dotnet ef` deben ejecutarse desde esa misma raíz, usando rutas al proyecto, por ejemplo:  
  `dotnet ef database update --project src/MundialitoCorporativo.Infrastructure --startup-project src/MundialitoCorporativo.Api`

## Run locally

### Backend

1. **SQL Server** (obligatorio). Una de estas opciones:
   - **Docker:** `docker run -d --name mundialito-db -e "ACCEPT_EULA=Y" -e "MSSQL_SA_PASSWORD=MundialitoSecurePwd123!" -p 1433:1433 mcr.microsoft.com/mssql/server:2022-latest`
   - O SQL Server local / LocalDB con la base `Mundialito`.
2. Connection string en `src/MundialitoCorporativo.Api/appsettings.json` (por defecto: `Server=localhost,1433;Database=Mundialito;User Id=sa;Password=MundialitoSecurePwd123!;TrustServerCertificate=True;`).
3. Ejecutar la API:
   ```bash
   cd src/MundialitoCorporativo.Api
   dotnet run
   ```
   En el primer arranque se aplican migraciones y seed (4 equipos, 5 jugadores por equipo, 6 partidos con 3 resultados).

   **Si la API no arranca** (error "Invalid object name 'Teams'" en logs): aplicar migraciones a mano una vez:
   ```bash
   dotnet tool install --global dotnet-ef   # solo la primera vez
   dotnet ef database update --project src/MundialitoCorporativo.Infrastructure --startup-project src/MundialitoCorporativo.Api
   ```
   Luego volver a ejecutar `dotnet run`.

**Probar sin levantar API (solo tests y build):**  
`.\scripts\run-and-test.ps1 -SkipApi`  
**Probar con API levantada:**  
`.\scripts\run-and-test.ps1` (hace GET /api/teams).

### Frontend

```bash
cd frontend
npm install
npm run dev
```

Open http://localhost:3000. Set `NEXT_PUBLIC_API_URL=http://localhost:5000` if the API is on another host/port.

### Docker (full stack)

```powershell
docker compose up --build -d
```

La primera vez tarda varios minutos (descarga de imágenes: SQL Server ~500 MB, .NET, Node). Cuando los contenedores estén en ejecución:

- **API:** http://localhost:5000 (Swagger: http://localhost:5000/swagger)
- **Frontend:** http://localhost:3000
- **DB:** localhost:1433 (usuario `sa`, contraseña en `docker-compose.yml`)

Probar que la API responde después de levantar Docker:

```powershell
.\scripts\test-api-after-docker.ps1
```

## Tests

```bash
dotnet test tests/MundialitoCorporativo.Tests/MundialitoCorporativo.Tests.csproj
```

## Postman

Import `postman/Mundialito-API.postman_collection.json`. Use variables `baseUrl` (e.g. http://localhost:5000) and `idempotencyKey` (e.g. `{{$guid}}`) for POST idempotency.

## Architecture and Git

- **docs/ARCHITECTURE.md** – Layers, CQRS, EF write path, Dapper read path, idempotency, Docker
- **docs/GIT_WORKFLOW.md** – development, release, main; atomic commits and Pull Requests

## API summary

| Method | Endpoint | Description |
|--------|----------|-------------|
| GET | /api/teams | List (pageNumber, pageSize, sortBy, sortDirection, name) |
| GET | /api/teams/{id} | Get by id |
| POST | /api/teams | Create (optional Idempotency-Key) |
| PUT | /api/teams/{id} | Full update |
| DELETE | /api/teams/{id} | Delete |
| GET | /api/players | List (teamId, name, pagination, sort) |
| GET | /api/players/{id} | Get by id |
| POST | /api/players | Create (Idempotency-Key supported) |
| PUT | /api/players/{id} | Full update |
| DELETE | /api/players/{id} | Delete |
| GET | /api/matches | List (teamId, dateFrom, dateTo, status, pagination) |
| GET | /api/matches/{id} | Get by id |
| POST | /api/matches | Create (Idempotency-Key supported) |
| PUT | /api/matches/{id} | Full update |
| PATCH | /api/matches/{id}/result | Set result (homeScore, awayScore) |
| DELETE | /api/matches/{id} | Delete |
| GET | /api/standings | League table |
| GET | /api/standings/top-scorers | Top scorers (?limit=10) |
