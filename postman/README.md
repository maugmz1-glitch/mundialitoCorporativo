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
- `idempotencyKey`: `{{$guid}}` (nuevo GUID por defecto). **Cada envío genera una clave distinta**, por eso cada "Create player" (o team, match, etc.) crea un recurso nuevo aunque el body sea igual.
- `idempotencyKeyFixed`: `test-same-key-1` — úsala si quieres **probar idempotencia**: en la request cambia la cabecera a `Idempotency-Key: {{idempotencyKeyFixed}}` y envía dos veces; la segunda vez recibirás la misma respuesta (sin crear otro jugador/equipo).
- `matchId`: ID de un partido (tras **List matches**, copia un `id` y asígnalo aquí para Get/Update/PATCH/cards/Delete).
- `token`: Se rellena automáticamente al ejecutar **Login**; necesario para **Me**.

### Idempotencia: por qué se crean “jugadores iguales”

La API identifica una petición idempotente por **(Idempotency-Key + método + ruta)**. No usa el body.  
En Postman, `idempotencyKey` está definido como `{{$guid}}`: **cada vez que envías una request, Postman genera un GUID nuevo**. Por tanto:

- Cada "Create player" tiene una clave distinta → la API lo trata como petición nueva → **crea un jugador cada vez** (es el comportamiento actual).
- Si quieres que al reenviar **no** se cree otro jugador: usa la **misma** clave en ambos envíos. Por ejemplo, en la request "Create player (idempotente)" cambia la cabecera a `Idempotency-Key: {{idempotencyKeyFixed}}`, envía una vez (201 + jugador creado), envía otra vez con el mismo body → recibirás el mismo 201 y el mismo body, y en base de datos seguirá habiendo un solo jugador.

### Uso

1. Importar `Mundialito.postman_collection.json` en Postman.
2. Ejecutar **Login** para poblar `token` (opcional, solo para **Me**).
3. Para probar Get/Update/Delete de un partido concreto: ejecutar **List matches**, copiar un `id` y asignarlo a la variable `matchId`.
