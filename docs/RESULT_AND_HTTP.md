# Patrón Result y mapeo HTTP

## Regla: no excepciones para control de flujo

El flujo de negocio **no** debe usar `throw` para errores esperados (validación, no encontrado, conflicto).  
Se usa el patrón **Result&lt;T&gt;** con:

- **Success(data)** → operación correcta.
- **Failure(mensaje, errorCode opcional)** → error de negocio; la API traduce el código a HTTP.

Así la API devuelve siempre respuestas coherentes (200/201/400/404/409) y el frontend puede mostrar mensajes sin depender de excepciones.

---

## Result&lt;T&gt; (Domain.Common)

```csharp
// Éxito
Result<TeamDto>.Success(dto)
Result.Success(dto)   // estático

// Fallo: mensaje + código opcional
Result<TeamDto>.Failure("Equipo no encontrado", ErrorCodes.NotFound)
Result.Failure<TeamDto>("Nombre de equipo requerido.", ErrorCodes.Validation)
```

Propiedades:

| Propiedad   | Tipo   | Descripción                          |
|------------|--------|--------------------------------------|
| IsSuccess  | bool   | true si la operación fue exitosa     |
| Data       | T?     | Datos cuando IsSuccess es true       |
| Message    | string | Mensaje de error cuando falla        |
| ErrorCode  | string?| Código opcional para mapear a HTTP   |

---

## ErrorCodes (Application.Common)

| Código     | Uso típico                    | HTTP en la API |
|------------|--------------------------------|----------------|
| Validation | Datos inválidos, reglas negocio | 400 Bad Request |
| NotFound  | Recurso no existe             | 404 Not Found   |
| Conflict  | Conflicto (ej. duplicado)     | 409 Conflict    |
| Duplicate | Alias de Conflict            | 409 Conflict    |

---

## Mapeo en la API (controllers)

Los controladores comprueban `result.IsSuccess` y, en caso de fallo, usan un método común **MapFailure** que traduce `ErrorCode` a status HTTP:

| HTTP        | Cuándo usarlo                          |
|-------------|----------------------------------------|
| **200 OK**  | GET/PUT/PATCH exitoso; cuerpo = result.Data |
| **201 Created** | POST crear recurso exitoso; Location + cuerpo |
| **204 No Content** | DELETE exitoso (sin cuerpo)        |
| **400 Bad Request** | ErrorCode Validation u otro; cuerpo `{ message, code }` |
| **404 Not Found**   | ErrorCode NotFound; cuerpo `{ message, code }` |
| **409 Conflict**    | ErrorCode Conflict/Duplicate; cuerpo `{ message, code }` |

Ejemplo en controller:

```csharp
var result = await _mediator.Send(new CreateTeamCommand(request.Name, request.LogoUrl), cancellationToken);
if (!result.IsSuccess) return MapFailure(result);
return CreatedAtAction(nameof(GetById), new { id = result.Data!.Id }, result.Data);
```

Implementación típica de `MapFailure`:

```csharp
private IActionResult MapFailure<T>(Result<T> result) =>
    result.ErrorCode switch
    {
        ErrorCodes.NotFound => NotFound(new { message = result.Message, code = result.ErrorCode }),
        ErrorCodes.Conflict or ErrorCodes.Duplicate => Conflict(new { message = result.Message, code = result.ErrorCode }),
        _ => BadRequest(new { message = result.Message, code = result.ErrorCode })
    };
```

---

## Ejemplo en un handler

```csharp
public async Task<Result<TeamDto>> Handle(CreateTeamCommand request, CancellationToken cancellationToken)
{
    if (string.IsNullOrWhiteSpace(request.Name))
        return Result.Failure<TeamDto>("El nombre del equipo es obligatorio.", ErrorCodes.Validation);

    var exists = await _db.Teams.AnyAsync(t => t.Name == request.Name.Trim(), cancellationToken);
    if (exists)
        return Result.Failure<TeamDto>("Ya existe un equipo con ese nombre.", ErrorCodes.Conflict);

    var team = new Team { ... };
    _db.Teams.Add(team);
    await _db.SaveChangesAsync(cancellationToken);
    return Result.Success(new TeamDto(team.Id, team.Name, ...));
}
```

Nunca se usa `throw` para “equipo no encontrado” o “nombre duplicado”; siempre `Result.Failure` con el código adecuado.
