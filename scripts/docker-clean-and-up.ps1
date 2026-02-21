# Deja solo los contenedores necesarios del proyecto: db, api, frontend.
# Elimina contenedores e imágenes huérfanas de este compose y vuelve a levantar los 3 servicios.
# Ejecutar desde la raíz del repositorio: .\scripts\docker-clean-and-up.ps1

$ErrorActionPreference = "Stop"
$root = Split-Path -Parent (Split-Path -Parent $PSScriptRoot)
if (-not (Test-Path (Join-Path $root "docker-compose.yml"))) {
    Write-Error "No se encontró docker-compose.yml. Ejecuta desde la raíz del repo o desde scripts."
    exit 1
}

Set-Location $root

Write-Host "Deteniendo y eliminando contenedores del proyecto..." -ForegroundColor Cyan
docker compose down

Write-Host "Levantando solo los servicios necesarios (db, api, frontend)..." -ForegroundColor Cyan
docker compose up -d --build

Write-Host "Listo. Contenedores: db (1433), api (5000), frontend (3000)." -ForegroundColor Green
Write-Host "Comprobar: docker compose ps" -ForegroundColor Gray
