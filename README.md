# Mundialito

Sistema de gestión de torneos: API .NET 8 (Clean Architecture, CQRS), frontend Next.js y SQL Server.

---

## Contenido

- [Ramas de GitHub](#ramas-de-github)
- [Stack](#stack)
- [Estructura del repositorio](#estructura-del-repositorio)
- [Requisitos](#requisitos)
- [Quick start](#quick-start)
- [Desarrollo local](#desarrollo-local)
- [Docker](#docker)
- [API](#api)
- [Documentación](#documentación)

---

## Stack

| Capa | Tecnología |
|------|------------|
| API | .NET 8, ASP.NET Core, MediatR (CQRS) |
| Escritura | Entity Framework Core 8, SQL Server |
| Lectura | Dapper (consultas optimizadas) |
| Frontend | Next.js 14, React, TypeScript |
| Infra | Docker, Docker Compose |

**Conceptos:** Result pattern, idempotencia (`Idempotency-Key` en POST), paginación y filtros en listados.

---

## Ramas de GitHub

| Rama | Descripción |
|------|-------------|
| **main** | Versión actual estable: equipos, jugadores, partidos, posiciones, proxy API en Docker. |
| **development** | Incluye todo lo de main más **gestión de árbitros** (CRUD y página Árbitros en el menú). |
| **release** | Incluye todo lo de main más **login básico** (usuario/contraseña, JWT, página Iniciar sesión y Cerrar sesión en la barra). Credenciales por defecto: `admin` / `Mundialito2024!`. |

---

## Estructura del repositorio

```
├── src/
│   ├── MundialitoCorporativo.Domain      # Entidades, Result, enums
│   ├── MundialitoCorporativo.Application # CQRS (commands, queries, handlers), DTOs
│   ├── MundialitoCorporativo.Infrastructure # EF Core, migraciones, Dapper, idempotencia
│   └── MundialitoCorporativo.Api        # Controllers, middleware, HTTP
├── tests/
│   └── MundialitoCorporativo.Tests      # Tests unitarios
├── frontend/                            # Next.js (equipos, jugadores, partidos, tabla)
├── docs/                                # Arquitectura, Git, guías
├── scripts/                             # PowerShell: push GitHub, tests, Docker
├── postman/                             # Colección Postman (API, idempotencia)
├── docker-compose.yml
├── global.json
└── MundialitoCorporativo.sln
```

---

## Requisitos

- **.NET 8 SDK**
- **Node 20+** (frontend)
- **SQL Server** (local o contenedor)
- **Docker y Docker Compose** (opcional, para stack completo)

Si el IDE muestra *"Unable to retrieve project metadata"*: abre la **carpeta raíz** (donde está el `.sln`), no una subcarpeta como `frontend`. Ejecuta `dotnet` desde esa raíz.

---

## Quick start

1. **Base de datos** (una opción):
   - Docker: `docker run -d --name mundialito-db -e "ACCEPT_EULA=Y" -e "MSSQL_SA_PASSWORD=MundialitoSecurePwd123!" -p 1433:1433 mcr.microsoft.com/mssql/server:2022-latest`
   - O SQL Server local con base `Mundialito`.
2. **API:** desde la raíz del repo:
   ```bash
   cd src/MundialitoCorporativo.Api
   dotnet run
   ```
   En el primer arranque se aplican migraciones y seed (equipos, jugadores, partidos).
3. **Frontend:** en otra terminal:
   ```bash
   cd frontend && npm install && npm run dev
   ```
4. Abrir **http://localhost:3000** y **http://localhost:5000/swagger**.

Si la API falla con *"Invalid object name 'Teams'"*: instalar `dotnet-ef` (`dotnet tool install --global dotnet-ef`) y ejecutar una vez  
`dotnet ef database update --project src/MundialitoCorporativo.Infrastructure --startup-project src/MundialitoCorporativo.Api`,  
luego volver a `dotnet run`.

---

## Desarrollo local

| Acción | Comando |
|--------|--------|
| Tests | `dotnet test tests/MundialitoCorporativo.Tests/MundialitoCorporativo.Tests.csproj` |
| Build | `dotnet build src/MundialitoCorporativo.Api/MundialitoCorporativo.Api.csproj -c Release` |
| Tests + build (+ opcional API) | `.\scripts\run-and-test.ps1` o `.\scripts\run-and-test.ps1 -SkipApi` |

Connection string por defecto: `Server=localhost,1433;Database=Mundialito;User Id=sa;Password=MundialitoSecurePwd123!;TrustServerCertificate=True;`  
(editable en `src/MundialitoCorporativo.Api/appsettings.json`).

---

## Docker

```powershell
docker compose up --build -d
```

- **API:** http://localhost:5000 — **Swagger:** http://localhost:5000/swagger  
- **Frontend:** http://localhost:3000  
- **SQL Server:** localhost:1433 (usuario `sa`, contraseña en `docker-compose.yml`)

Comprobar API tras levantar: `.\scripts\test-api-after-docker.ps1`

---

## API

| Método | Ruta | Descripción |
|--------|------|-------------|
| GET | `/api/teams` | Lista (paginación, orden, filtro por nombre) |
| GET | `/api/teams/{id}` | Detalle equipo |
| POST | `/api/teams` | Crear (opcional `Idempotency-Key`) |
| PUT | `/api/teams/{id}` | Actualizar |
| DELETE | `/api/teams/{id}` | Eliminar |
| GET | `/api/players` | Lista (teamId, nombre, paginación) |
| GET/POST/PUT/DELETE | `/api/players`, `/api/players/{id}` | CRUD jugadores |
| GET | `/api/matches` | Lista (teamId, fechas, estado, paginación) |
| PATCH | `/api/matches/{id}/result` | Establecer resultado (homeScore, awayScore) |
| GET | `/api/standings` | Tabla de posiciones |
| GET | `/api/standings/top-scorers` | Goleadores (`?limit=10`) |

Respuestas: 200, 201, 204, 400, 404, 409. Formato paginado: `{ data, pageNumber, pageSize, totalRecords }`.

---

## Documentación

| Documento | Contenido |
|-----------|-----------|
| [docs/README.md](docs/README.md) | Índice de toda la documentación |
| [docs/ARCHITECTURE.md](docs/ARCHITECTURE.md) | Capas, CQRS, EF vs Dapper, idempotencia, Docker |
| [docs/GIT_WORKFLOW.md](docs/GIT_WORKFLOW.md) | Ramas main/development/release, PRs, releases |
| [docs/SUBIR_A_GITHUB.md](docs/SUBIR_A_GITHUB.md) | Crear repo y subir ramas (script `scripts/push-to-github.ps1`) |
| [scripts/README.md](scripts/README.md) | Descripción de los scripts PowerShell (push, tests, Docker) |

Postman: importar `postman/Mundialito-API.postman_collection.json` (variables: `baseUrl`, `idempotencyKey`).

---

## Subir a GitHub

Repositorio con ramas **main**, **development** y **release**. Tras crear un repo vacío en GitHub:

```powershell
git remote add origin https://github.com/TU_USUARIO/TU_REPO.git
git push -u origin main
git push -u origin development
git push -u origin release
```

O: `.\scripts\push-to-github.ps1 -GitHubUrl "https://github.com/TU_USUARIO/TU_REPO.git"`  
Detalle en [docs/SUBIR_A_GITHUB.md](docs/SUBIR_A_GITHUB.md).
