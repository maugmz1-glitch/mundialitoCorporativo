# Postman – Mundialito API

## Colección

- **`Mundialito.postman_collection.json`** – Colección con todos los endpoints y casos requeridos.

### Contenido

| Requisito | Cumplimiento |
|-----------|--------------|
| **Todos los endpoints** | Auth (register, login, me), Teams, Players, Matches, Referees, Standings (tabla y top-scorers). Incluye GET por id, list, POST create, PUT update, PATCH result, POST cards, DELETE. |
| **Idempotencia** | POST con cabecera `Idempotency-Key` en: Create team, Create player, Create match, Add card, Create referee. Variable `{{idempotencyKey}}` = `{{$guid}}`. |
| **Filtros** | Teams (name), Players (teamId), Matches (status, teamId, dateFrom, dateTo), Referees (name). |
| **Paginación** | Listados con `pageNumber`, `pageSize`, `sortBy`, `sortDirection` en Teams, Players, Matches, Referees. |
| **Manejo de errores** | 404 (recurso no encontrado), 400 (validación nombre vacío), 409 (equipo duplicado), 404 player. |

### Variables de colección

- `baseUrl`: `http://localhost:5000` (API en local).
- `idempotencyKey`: `{{$guid}}` (nuevo GUID por defecto).
- `matchId`: ID de un partido (tras **List matches**, copia un `id` de la respuesta y pégalo en Variables de colección para Get/Update/PATCH/cards/Delete).
- `token`: Se rellena automáticamente al ejecutar **Login** (script de tests); necesario para **Me**.

### Uso

1. Importar `Mundialito.postman_collection.json` en Postman.
2. Ejecutar **Login** para poblar `token` (opcional, solo para **Me**).
3. Para probar Get/Update/Delete de un partido concreto: ejecutar **List matches**, copiar un `id` y asignarlo a la variable `matchId`.
