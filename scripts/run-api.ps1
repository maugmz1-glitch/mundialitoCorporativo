# Ejecuta la API desde la raíz del repo (evita "find a project to run").
# Requiere SQL Server en localhost:1433 (p. ej. docker-compose up -d db).

$ErrorActionPreference = "Stop"
$root = Split-Path -Parent (Split-Path -Parent $PSScriptRoot)
Push-Location $root
try {
    dotnet run --project src/MundialitoCorporativo.Api
} finally {
    Pop-Location
}
