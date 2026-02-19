# Script para ejecutar tests, compilar y (opcional) probar la API.
# La API requiere SQL Server en localhost:1433 (o Docker: docker run -d -p 1433:1433 -e "ACCEPT_EULA=Y" -e "MSSQL_SA_PASSWORD=MundialitoSecurePwd123!" mcr.microsoft.com/mssql/server:2022-latest)

param(
    [switch]$SkipApi  # Si no tienes SQL Server, usa -SkipApi para solo tests y build
)

$ErrorActionPreference = "Stop"
$root = Split-Path -Parent (Split-Path -Parent $PSScriptRoot)

Write-Host "=== 1. Tests unitarios ===" -ForegroundColor Cyan
Push-Location $root
dotnet test tests/MundialitoCorporativo.Tests/MundialitoCorporativo.Tests.csproj --verbosity minimal
if ($LASTEXITCODE -ne 0) { Pop-Location; exit 1 }
Pop-Location

Write-Host "`n=== 2. Build Release ===" -ForegroundColor Cyan
dotnet build $root/src/MundialitoCorporativo.Api/MundialitoCorporativo.Api.csproj -c Release --verbosity minimal
if ($LASTEXITCODE -ne 0) { exit 1 }

if ($SkipApi) {
    Write-Host "`n(Omitiendo arranque de API: -SkipApi). Para probar la API, inicia SQL Server y ejecuta: dotnet run --project $root\src\MundialitoCorporativo.Api" -ForegroundColor Yellow
    exit 0
}

Write-Host "`n=== 3. Probar API (GET /api/teams) ===" -ForegroundColor Cyan
$apiUrl = "http://localhost:5000/api/teams"
try {
    $r = Invoke-RestMethod -Uri $apiUrl -Method Get -TimeoutSec 5
    Write-Host "OK. Equipos: $($r.data.Count); totalRecords: $($r.totalRecords)" -ForegroundColor Green
} catch {
    Write-Host "No se pudo conectar a la API en $apiUrl. ¿Está SQL Server en localhost:1433 y la API en ejecución (dotnet run)?" -ForegroundColor Yellow
    Write-Host $_.Exception.Message
    exit 1
}

Write-Host "`nTodo correcto." -ForegroundColor Green
