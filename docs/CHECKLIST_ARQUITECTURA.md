# Checklist: elementos de arquitectura y buenas prácticas

Revisión del proyecto contra los criterios solicitados.

---

## 1. Clean Architecture

| Criterio | Estado | Evidencia |
|----------|--------|-----------|
| Capas bien definidas | ✅ Cumple | **Domain** (sin referencias), **Application** (solo Domain), **Infrastructure** (Application + Domain), **Api** (Application + Infrastructure). |
| Domain sin dependencias externas | ✅ Cumple | `MundialitoCorporativo.Domain.csproj` no referencia otros proyectos ni paquetes de infraestructura. |
| Application sin referencias a Infrastructure | ✅ Cumple | Application solo referencia Domain; define interfaces (`IAppDbContext`, `ITeamReadRepository`, etc.) implementadas en Infrastructure. |
| API orquesta sin lógica de negocio | ✅ Cumple | Controllers solo envían Commands/Queries vía MediatR y mapean Result a HTTP. |

**Conclusión:** Clean Architecture aplicada correctamente (capas, dependencias, interfaces en Application).

---

## 2. CQRS

| Criterio | Estado | Evidencia |
|----------|--------|-----------|
| Separación Commands / Queries | ✅ Cumple | Commands: `CreateTeam`, `UpdateTeam`, `DeleteTeam`, `CreatePlayer`, `CreateMatch`, `SetMatchResult`, etc. Queries: `GetTeams`, `GetTeamById`, `GetPlayers`, `GetMatches`, `GetStandings`, `GetTopScorers`. |
| MediatR como bus | ✅ Cumple | `AddMediatR` en Application; controllers usan `_mediator.Send(command/query)`. |
| Handlers dedicados | ✅ Cumple | Un handler por comando/consulta (p. ej. `CreateTeamCommandHandler`, `GetTeamsQueryHandler`). |

**Conclusión:** CQRS implementado con MediatR y handlers separados por operación.

---

## 3. Escritura con EF Core

| Criterio | Estado | Evidencia |
|----------|--------|-----------|
| Escrituras vía DbContext | ✅ Cumple | Todos los command handlers usan `IAppDbContext`: `_db.Teams.Add`, `_db.Players.Add`, `_db.Matches.Add`, `SaveChangesAsync`. |
| Interfaz en Application | ✅ Cumple | `IAppDbContext` en Application (DbSets de Team, Player, Match, MatchGoal, IdempotencyRecord). |
| Implementación en Infrastructure | ✅ Cumple | `AppDbContext` en Infrastructure con EF Core, migraciones y configuración. |

**Conclusión:** Toda la escritura pasa por EF Core mediante `IAppDbContext`.

---

## 4. Lectura optimizada con Dapper

| Criterio | Estado | Evidencia |
|----------|--------|-----------|
| Lecturas vía Dapper | ✅ Cumple | `TeamReadRepository`, `PlayerReadRepository`, `MatchReadRepository`, `StandingsReadRepository` usan `SqlConnection` + `QueryAsync` / `QuerySingleOrDefaultAsync`. |
| Sin EF en consultas | ✅ Cumple | GetById y listas (incl. paginación) resueltos con Dapper en los *ReadRepository*. |
| Paginación en BD | ✅ Cumple | `OFFSET/FETCH` en SQL (p. ej. en `TeamReadRepository.GetPagedAsync`). |

**Conclusión:** Lecturas optimizadas con Dapper; listas paginadas en base de datos.

---

## 5. Result Pattern

| Criterio | Estado | Evidencia |
|----------|--------|-----------|
| Sin excepciones para flujo de negocio | ✅ Cumple | Handlers devuelven `Result.Success(data)` o `Result.Failure(message, errorCode)`. |
| Success / Failure / mensajes / códigos | ✅ Cumple | `Result<T>` en Domain con `IsSuccess`, `Data`, `Message`, `ErrorCode`; `ErrorCodes.Validation`, `NotFound`, `Conflict`, `Duplicate` en Application. |
| Mapeo consistente en API | ✅ Cumple | `ResultExtensions.ToActionResult()` traduce a 400/404/409; controllers usan `result.ToActionResult()`. |

**Conclusión:** Result Pattern aplicado de punta a punta; documentado en `docs/RESULT_AND_HTTP.md`.

---

## 6. REST correcto e idempotente

| Criterio | Estado | Evidencia |
|----------|--------|-----------|
| Verbos HTTP correctos | ✅ Cumple | GET (recursos y listas), POST (crear), PUT (reemplazar), PATCH (resultado partido), DELETE (eliminar). |
| Códigos de estado | ✅ Cumple | 200 OK, 201 Created (+ `CreatedAtAction`), 204 No Content (DELETE), 400/404/409 desde Result. |
| Idempotencia en POST | ✅ Cumple | `IdempotencyMiddleware` + cabecera `Idempotency-Key`; respuestas guardadas en `IdempotencyRecords`; mismo key → misma respuesta sin re-ejecutar. |
| Frontend envía Idempotency-Key | ✅ Cumple | `postApi(..., idempotencyKey)` en `api.ts`; equipos/jugadores/partidos usan clave en creación. |

**Conclusión:** REST alineado con buenas prácticas e idempotencia operativa en POST.

---

## 7. Buen versionado

| Criterio | Estado | Evidencia |
|----------|--------|-----------|
| Ruta con versión de API | ✅ Cumple | Rutas `api/v1/[controller]` (p. ej. `api/v1/teams`). Paquete `Asp.Versioning.Mvc` 8.1.1. |
| Configuración en backend | ✅ Cumple | `AddApiVersioning` en Program.cs: versión por defecto 1.0, `AssumeDefaultVersionWhenUnspecified`, `ReportApiVersions`. |
| Frontend usa v1 | ✅ Cumple | `frontend/src/lib/api.ts`: `versionedPath()` reescribe `/api/...` → `/api/v1/...` en todas las peticiones. |

**Conclusión:** Versionado implementado; rutas expuestas como `api/v1/teams`, `api/v1/players`, etc.

---

## 8. Frontend desacoplado en Next.js

| Criterio | Estado | Evidencia |
|----------|--------|-----------|
| Proyecto Next.js independiente | ✅ Cumple | Carpeta `frontend/` con `package.json`, Next 14, React 18; sin referencias al backend .NET. |
| Comunicación solo por HTTP | ✅ Cumple | `fetch` a `${API}${path}`; `api.ts` con `fetchApi`, `fetchPaged`, `postApi`, `putApi`, `patchApi`, `deleteApi`. |
| URL de API configurable | ✅ Cumple | `NEXT_PUBLIC_API_URL`; `next.config.js` proxy a ese origen para `/api/*`. |
| Sin lógica de negocio duplicada | ✅ Cumple | Frontend solo consume endpoints REST y muestra datos; validación y reglas en backend. |

**Conclusión:** Frontend desacoplado en Next.js, solo cliente HTTP y configuración por entorno.

---

## Resumen

| Elemento | Estado |
|----------|--------|
| Clean Architecture | ✅ Cumple |
| CQRS | ✅ Cumple |
| Escritura con EF Core | ✅ Cumple |
| Lectura optimizada con Dapper | ✅ Cumple |
| Result Pattern | ✅ Cumple |
| REST correcto e idempotente | ✅ Cumple |
| Buen versionado | ✅ Cumple |
| Frontend desacoplado en Next.js | ✅ Cumple |

**Versionado:** Implementado con `Asp.Versioning.Mvc`; rutas `api/v1/...` y frontend actualizado para usarlas.
