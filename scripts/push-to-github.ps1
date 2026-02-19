# Script para subir el proyecto a GitHub (ejecutar desde la raíz del repo).
# Uso: .\scripts\push-to-github.ps1 -GitHubUrl "https://github.com/TU_USUARIO/TU_REPO.git"
# O:   .\scripts\push-to-github.ps1 -GitHubUrl "git@github.com:TU_USUARIO/TU_REPO.git"

param(
    [Parameter(Mandatory = $true)]
    [string]$GitHubUrl
)

$ErrorActionPreference = "Stop"
$root = Split-Path -Parent (Split-Path -Parent $PSScriptRoot)

Push-Location $root

if (-not (Test-Path .git)) {
    Write-Error "No hay repositorio Git en $root. Ejecuta primero: git init"
}

# Si ya existe origin, preguntar si reemplazar
$remote = git remote get-url origin 2>$null
if ($remote) {
    Write-Host "Ya existe un remoto 'origin': $remote" -ForegroundColor Yellow
    $r = Read-Host "¿Reemplazar por $GitHubUrl? (s/n)"
    if ($r -ne 's') { Pop-Location; exit 0 }
    git remote remove origin
}

git remote add origin $GitHubUrl
Write-Host "Remoto 'origin' configurado: $GitHubUrl" -ForegroundColor Green

Write-Host "`nSubiendo ramas main, development, release..." -ForegroundColor Cyan
git push -u origin main
git push -u origin development
git push -u origin release

Write-Host "`nListo. Repositorio subido a GitHub." -ForegroundColor Green
Pop-Location
