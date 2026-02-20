# Ejecutar despues de: docker compose up --build -d
# Espera a que la API responda y prueba GET /api/teams y GET /api/standings

param(
    [string]$BaseUrl = "http://localhost:5000",
    [int]$MaxAttempts = 30,
    [int]$SleepSeconds = 5
)

$ErrorActionPreference = "Stop"
Write-Host "Esperando API en $BaseUrl (max ${MaxAttempts}x ${SleepSeconds}s)..." -ForegroundColor Cyan

for ($i = 1; $i -le $MaxAttempts; $i++) {
    try {
        $r = Invoke-RestMethod -Uri "$BaseUrl/api/teams" -Method Get -TimeoutSec 5
        Write-Host "API lista (intento $i). Equipos: $($r.data.Count), totalRecords: $($r.totalRecords)" -ForegroundColor Green
        Write-Host "`n--- GET /api/standings ---" -ForegroundColor Cyan
        $s = Invoke-RestMethod -Uri "$BaseUrl/api/standings" -Method Get
        $s | ConvertTo-Json -Depth 3
        Write-Host "`nTodo OK. Frontend: http://localhost:3000  Swagger: $BaseUrl/swagger" -ForegroundColor Green
        exit 0
    } catch {
        Write-Host "  Intento $i/$MaxAttempts - no disponible: $($_.Exception.Message)" -ForegroundColor Yellow
        if ($i -lt $MaxAttempts) { Start-Sleep -Seconds $SleepSeconds }
    }
}
Write-Host "La API no respondio a tiempo. Comprueba: docker compose ps -a" -ForegroundColor Red
exit 1
