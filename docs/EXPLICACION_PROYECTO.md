# Explicación del proyecto Mundialito – Paso a paso

Este documento describe qué se ha implementado, en qué orden y por qué. Al final se resumen las **operaciones más importantes** del proyecto.

---

## 1. Estructura de la solución (Clean Architecture)

Se creó una solución con **cuatro capas** más tests:

| Capa | Proyecto | Responsabilidad |
|------|----------|-----------------|
| **Domain** | MundialitoCorporativo.Domain | Entidades, enums, patrón Result. Sin dependencias externas. |
| **Application** | MundialitoCorporativo.Application | Casos de uso: comandos, consultas, DTOs, interfaces (IAppDbContext, I*ReadRepository). Depende solo de Domain. |
| **Infrastructure** | MundialitoCorporativo.Infrastructure | Implementaciones: EF Core (escrituras), Dapper (lecturas), migraciones, seed, idempotencia. Depende de Application y Domain. |
| **Api** | MundialitoCorporativo.Api | Controladores, middleware, configuración HTTP. Punto de entrada. Depende de Application e Infrastructure. |
| **Tests** | MundialitoCorporativo.Tests | Tests unitarios (standings, idempotencia). |

**Por qué:** Separación clara de responsabilidades: la lógica de negocio está en Application; la persistencia en Infrastructure; la API solo orquesta y mapea HTTP.

---

## 2. Capa Domain

### 2.1 Entidades

- **Team**: Id, Name, LogoUrl, fechas. Navegación a Players y Matches (home/away).
- **Player**: Id, TeamId, FirstName, LastName, JerseyNumber, Position. Pertenece a un Team.
- **Match**: HomeTeamId, AwayTeamId, ScheduledAtUtc, Venue, Status (enum), HomeScore, AwayScore.
- **MatchGoal**: Para futuros goles detallados (jugador, minuto, autogol).
- **IdempotencyRecord**: IdempotencyKey, RequestMethod, RequestPath, ResponseStatusCode, ResponseBody. Guarda la respuesta de un POST para no duplicar en reintentos.

### 2.2 Patrón Result

- **Result&lt;T&gt;** con `IsSuccess`, `Data`, `Message`, `ErrorCode`.
- **Result.Success(data)** y **Result.Failure(message, errorCode)**.
- **Objetivo:** No usar excepciones para flujo de negocio; la API traduce Result a códigos HTTP (200, 201, 400, 404, 409).

---

## 3. Capa Application (CQRS)

### 3.1 Comandos (escrituras)

- Cada operación que modifica datos es un **Command** (record) y un **CommandHandler**.
- El handler usa **IAppDbContext** (EF Core): Add, Find, Update, Remove, SaveChangesAsync.
- Ejemplos: CreateTeamCommand, UpdateTeamCommand, DeleteTeamCommand, CreatePlayerCommand, CreateMatchCommand, SetMatchResultCommand, etc.
- Validaciones en el handler (nombre vacío, equipo no encontrado, etc.) devuelven **Result.Failure** con el código de error adecuado.

### 3.2 Consultas (lecturas)

- Cada lectura es una **Query** (o record) y un **QueryHandler**.
- El handler usa **I*ReadRepository** (Dapper): GetByIdAsync, GetPagedAsync con filtros, ordenación y paginación en base de datos.
- Ejemplos: GetTeamsQuery, GetTeamByIdQuery, GetPlayersQuery, GetMatchesQuery, GetStandingsQuery, GetTopScorersQuery.
- Las listas devuelven **PagedResult&lt;T&gt;** (Data, PageNumber, PageSize, TotalRecords, TotalPages).

### 3.3 MediatR

- Todas las peticiones (Command/Query) se envían con **IMediator.Send(request)**.
- MediatR resuelve el handler correcto. Los handlers devuelven **Result&lt;T&gt;** (no excepciones).

### 3.4 Interfaces de lectura

- **ITeamReadRepository**, **IPlayerReadRepository**, **IMatchReadRepository**, **IStandingsReadRepository**, **IIdempotencyStore**.
- Definidas en Application; implementadas en Infrastructure. Así la capa de aplicación no conoce SQL ni Dapper.

---

## 4. Capa Infrastructure

### 4.1 EF Core (solo escrituras)

- **AppDbContext** implementa **IAppDbContext**: DbSets de Team, Player, Match, MatchGoal, IdempotencyRecord.
- Configuración de entidades (longitud de campos, FKs, índices). Migración inicial en **Migrations/**.
- **Todas** las inserciones, actualizaciones y borrados pasan por este DbContext.

### 4.2 Dapper (solo lecturas)

- **TeamReadRepository**, **PlayerReadRepository**, **MatchReadRepository**: consultas SQL con **OFFSET/FETCH** para paginación, filtros (nombre, teamId, fecha, status) y ORDER BY dinámico (sortBy, sortDirection).
- **StandingsReadRepository**: una sola consulta SQL (CTE) que:
  - Considera solo partidos completados (Status = 2).
  - Por cada equipo calcula: partidos jugados, ganados, empatados, perdidos, goles a favor/en contra, puntos (3/1/0).
  - Ordena por: **Points DESC, GoalDifferential DESC, GoalsFor DESC** (reglas del torneo).
- **Top scorers:** consulta sobre MatchGoals (por ahora sin datos de goles en seed; la estructura está lista).

### 4.3 Idempotencia

- **IdempotencyStore** implementa **IIdempotencyStore** usando la tabla **IdempotencyRecords** (EF).
- **GetAsync(key, method, path):** si ya existe respuesta guardada para esa clave+método+ruta, se devuelve (StatusCode + Body).
- **StoreAsync:** después de ejecutar el POST, se guarda status code y cuerpo de la respuesta para esa clave.

### 4.4 Seed

- **SeedData.EnsureSeedAsync**: se ejecuta al arrancar la API.
  - Aplica migraciones pendientes.
  - Si ya hay equipos, no hace nada.
  - Si no, inserta: 4 equipos, 5 jugadores por equipo, 6 partidos (3 con resultado completado).

---

## 5. Capa API

### 5.1 Program.cs

- Registro de **Application** (MediatR) e **Infrastructure** (DbContext, repositorios, IdempotencyStore).
- **AddControllers** con **camelCase** en JSON.
- **Swagger** en desarrollo.
- **CORS** para el frontend (localhost:3000).
- **EnsureSeedAsync()** al iniciar.
- **IdempotencyMiddleware** antes de CORS y controladores.
- **MapControllers** para las rutas.

### 5.2 Controladores

- Cada controlador recibe **IMediator**.
  - **GET por id:** Send(GetXxxByIdQuery(id)) → si Result.Data es null → 404; si no → 200 + Data.
  - **GET lista:** Send(GetXxxQuery con query params) → 200 + PagedResponse (data, pageNumber, pageSize, totalRecords, totalPages).
  - **POST:** Send(CreateXxxCommand(...)) → si falla → MapFailure (400/404/409); si ok → 201 Created + ubicación.
  - **PUT:** Send(UpdateXxxCommand(...)) → MapFailure o 200 + Data.
  - **DELETE:** Send(DeleteXxxCommand(...)) → MapFailure o 204 No Content.
  - **PATCH /api/matches/{id}/result:** Send(SetMatchResultCommand(id, homeScore, awayScore)) → 200 + MatchDto.

- **MapFailure(result):** según **result.ErrorCode** (NotFound → 404, Conflict/Duplicate → 409, resto → 400), devolviendo body con message y code.

### 5.3 IdempotencyMiddleware

- Solo actúa en **POST** y si existe cabecera **Idempotency-Key**.
- Busca en **IIdempotencyStore** respuesta previa para (key, method, path).
  - Si existe: devuelve **el mismo** status code y body, **sin** ejecutar el handler (evita duplicados en reintentos).
  - Si no existe: deja pasar la petición, captura la respuesta en un MemoryStream, la envía al cliente, y guarda (status + body) en el store para esa clave.

---

## 6. Frontend (Next.js)

- **App Router** con páginas: Home, Teams, Players, Matches, Standings.
- **Teams:** listado paginado, filtro por nombre, ordenación; formulario crear/editar; borrar.
- **Players:** listado con filtro por equipo; crear/editar/borrar (siempre asociado a un equipo).
- **Matches:** listado con filtro por estado; crear partido; botón “Set result” que hace PATCH al resultado.
- **Standings:** tabla (posición, equipo, P, W, D, L, GF, GA, GD, Pts) y tabla de top scorers.
- Las llamadas a la API usan **NEXT_PUBLIC_API_URL**; en POST se puede enviar **Idempotency-Key** (por ejemplo en creación de equipos/jugadores/partidos).

---

## 7. Docker

- **docker-compose.yml:** servicios `db` (SQL Server 2022), `api` (Dockerfile del Api), `frontend` (Dockerfile de Next.js). La API espera a que la base esté disponible.
- **docker-compose.override.yml:** healthcheck simplificado para la base si no hay sqlcmd en la imagen.

---

## 8. Tests y Postman

- **StandingsLogicTests:** reglas de puntos (3/1/0), diferencia de goles, orden (puntos → GD → GF), cálculo de totalPages.
- **IdempotencyTests:** IdempotencyStore con base en memoria; guardar y recuperar respuesta por clave.
- **Postman:** colección con todos los endpoints, filtros, paginación, idempotencia (Idempotency-Key en POST) y casos de error (404, 400).

---

## 9. Operaciones más importantes (resumen)

1. **Result pattern (Domain):** Todo el flujo de negocio devuelve Result&lt;T&gt;. Sin excepciones para validaciones; la API traduce a HTTP.
2. **CQRS (Application):** Comandos → IAppDbContext (EF); Consultas → I*ReadRepository (Dapper). Un solo lugar por tipo de operación.
3. **Paginación en base de datos:** Listas con OFFSET/FETCH y COUNT; nunca cargar todos los registros en memoria.
4. **Standings en una consulta:** Un solo SQL (CTE) calcula puntos, GD, GF y ordena; Dapper mapea a StandingRowDto.
5. **Idempotencia:** Middleware + store; POST con Idempotency-Key devuelve la misma respuesta en reintentos sin duplicar recursos.
6. **Seed al arrancar:** Migrations + datos iniciales (4 equipos, 5 jugadores/equipo, 6 partidos, 3 con resultado) en el primer run.
7. **REST y códigos HTTP:** 200, 201, 204, 400, 404, 409 según Result; formato de lista estándar (data, pageNumber, pageSize, totalRecords, totalPages).

Con esto se tiene el recorrido completo del proyecto y el porqué de cada decisión. Para más detalle técnico de capas y flujos, ver **docs/ARCHITECTURE.md**.
