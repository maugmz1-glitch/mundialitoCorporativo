# Mundialito

Sistema de gestión de torneos: API .NET 8 (Clean Architecture, CQRS), frontend Next.js y SQL Server.

---

## Contenido

- [Stack](#stack)
- [Cumplimiento de requisitos](#cumplimiento-de-requisitos)
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

## Cumplimiento de requisitos

| Requisito | Cumplimiento |
|-----------|--------------|
| **Clean Architecture** | Capas: `Domain` (sin dependencias) → `Application` (solo Domain) → `Infrastructure` (Application + Domain) → `Api` (Application + Infrastructure). La API no referencia Domain directamente. |
| **CQRS** | Comandos (escritura) y queries (lectura) con MediatR; handlers separados (`*CommandHandler`, `*QueryHandler`). |
| **Escritura con EF Core** | Solo EF: `IAppDbContext`, DbContext, migraciones. Command handlers usan `_db.Add`, `FindAsync`, `SaveChangesAsync`. |
| **Lectura solo Dapper** | No se usa EF para lecturas. Todas las queries usan `I*ReadRepository` (Dapper + SQL): Team, Player, Match, Standings, Referee. |
| **Sin excepciones para control de flujo** | Los handlers no lanzan excepciones para flujo de negocio; devuelven `Result.Success(data)` o `Result.Failure("mensaje", ErrorCodes.X)`. |
| **Result Pattern** | `Domain.Common.Result<T>`: `Success(data)`, `Failure(message, errorCode?)`, `Message`, `ErrorCode`. Códigos: `ErrorCodes.NotFound`, `Validation`, `Conflict`, `Duplicate`. |
| **Mapeo HTTP** | Éxito: 200 OK, 201 Created (`CreatedAtAction`), 204 No Content. Error vía `ToActionResult()`: 400 Bad Request, 404 NotFound, 409 Conflict. |
| **REST correcto e idempotente** | GET (list/ById), POST (201 + `CreatedAtAction`), PUT (actualización completa), PATCH (p. ej. `result`), DELETE (204). Idempotencia en POST mediante `IdempotencyMiddleware` y cabecera `Idempotency-Key`. |
| **Buen versionado** | `Asp.Versioning.Mvc`; rutas `api/v{version:apiVersion}/[controller]` y `[ApiVersion("1.0")]`; documentado en README/API. |
| **Frontend desacoplado en Next.js** | App Next.js en `frontend/`; consume solo HTTP (proxy a la API); sin lógica de negocio ni referencias a proyectos .NET. |

**Result pattern y códigos HTTP**

- No se usan excepciones para control de flujo; los handlers devuelven `Result<T>`.
- Ejemplo en aplicación: `Result.Success(team)` o `Result.Failure("Equipo no encontrado", ErrorCodes.NotFound)`.
- En la API: si `!result.IsSuccess` se devuelve `result.ToActionResult()`; si éxito, el controlador devuelve:
  - **200 OK** (GET por id, GET list, PUT, PATCH),
  - **201 Created** (POST con `CreatedAtAction`),
  - **204 No Content** (DELETE).
- `ToActionResult()` según `ErrorCode`: **404** NotFound, **409** Conflict/Duplicate, **400** resto (p. ej. Validation).

**Métodos HTTP (REST)**

| Método | Uso en la API | Ejemplo |
|--------|----------------|---------|
| **GET** | Obtener recursos (por id o listado) | `GET /api/teams`, `GET /api/teams/{id}`, `GET /api/standings`, `GET /api/standings/top-scorers` |
| **POST** | Crear recursos (201 Created) | `POST /api/teams`, `POST /api/matches`, `POST /api/matches/{id}/cards` (subrecurso) |
| **PUT** | Reemplazo completo del recurso (idempotente) | `PUT /api/teams/{id}`, `PUT /api/players/{id}` con cuerpo completo |
| **PATCH** | Modificación parcial | `PATCH /api/matches/{id}/result` (solo resultado; resto del partido no se toca) |
| **DELETE** | Eliminación (idempotente; 204 No Content) | `DELETE /api/teams/{id}`; repetir el mismo DELETE → 404 |

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
2. **API** (una de las dos formas):
   - Desde la raíz del repo: `dotnet run --project src/MundialitoCorporativo.Api`
   - O entrando a la carpeta: `cd src/MundialitoCorporativo.Api` y luego `dotnet run`
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

Solo son necesarios **tres contenedores**: `db`, `api`, `frontend`. Si tienes contenedores duplicados o viejos:

```powershell
.\scripts\docker-clean-and-up.ps1
```

Eso detiene y elimina los del proyecto y vuelve a levantar solo esos tres. Para levantar sin limpiar:

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

## Tests

`dotnet test tests/MundialitoCorporativo.Tests/MundialitoCorporativo.Tests.csproj`

| Área | Cobertura |
|------|-----------|
| **Tabla de posiciones** | Cálculo de puntos (3/1/0), diferencia de gol (GF−GA), orden: Points → GoalDifferential → GoalsFor, desempate por GD y GF. |
| **Idempotencia** | IdempotencyStore: Get sin registro devuelve null; Store+Get devuelve la respuesta guardada; misma clave devuelve la misma respuesta dos veces; distinto método/path no devuelve el registro. |
| **Result Pattern** | Success (IsSuccess, Data, Message vacío, ErrorCode null), Failure (mensaje, ErrorCode opcional), todos los ErrorCodes (NotFound, Validation, Conflict, Duplicate). |

El mapeo Result → HTTP (400/404/409) en la API se verifica con la colección Postman (casos en «Manejo de errores»).

---

## Documentación

| Documento | Contenido |
|-----------|-----------|
| [docs/README.md](docs/README.md) | Índice de toda la documentación |
| [docs/ARCHITECTURE.md](docs/ARCHITECTURE.md) | Capas, CQRS, EF vs Dapper, idempotencia, Docker |
| [docs/arquitectura mundilito.png](docs/arquitectura%20mundilito.png) | Capas, CQRS, EF vs Dapper, idempotencia, Docker |
| [docs/EXPLICACION_PROYECTO.md](docs/EXPLICACION_PROYECTO.md) | Capas, CQRS, EF vs Dapper, idempotencia, Docker |
| [docs/RESULT_AND_HTTP.md](docs/RESULT_AND_HTTP.md) | Capas, CQRS, EF vs Dapper, idempotencia, Docker |
| 


Postman: importar `postman/Mundialito.postman_collection.json` (ver [postman/README.md](postman/README.md); variables: `baseUrl`, `idempotencyKey`, `matchId`, `token`).

---

## Subir a GitHub

Requisitos típicos: **varios commits** (no uno solo), **Pull Requests** para integrar en `development`, **commits atómicos y claros**. Detalle en [docs/GIT_WORKFLOW.md](docs/GIT_WORKFLOW.md).

Repositorio con ramas **main**, **development** y **release**. Tras crear un repo vacío en GitHub:

```powershell
git remote add origin https://github.com/TU_USUARIO/TU_REPO.git
git push -u origin main
git push -u origin development
git push -u origin release
```

O: `.\scripts\push-to-github.ps1 -GitHubUrl "https://github.com/TU_USUARIO/TU_REPO.git"`  
Detalle en [docs/SUBIR_A_GITHUB.md](docs/SUBIR_A_GITHUB.md).
