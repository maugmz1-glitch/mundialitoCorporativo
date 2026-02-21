# Scripts

Scripts PowerShell para automatizar tareas habituales. Ejecutar desde la **raíz del repositorio**.

| Script | Uso |
|--------|-----|
| **docker-clean-and-up.ps1** | Deja solo los 3 contenedores necesarios: para todo, ejecuta `docker compose down` y luego `docker compose up -d --build` (db, api, frontend). |
| **push-to-github.ps1** | Configura el remoto `origin` y sube las ramas `main`, `development` y `release` a GitHub. Requiere: `git init` y al menos un commit. |
| **run-and-test.ps1** | Ejecuta tests unitarios, build Release de la API y (opcional) prueba GET `/api/teams`. Usar `-SkipApi` si no hay SQL Server. |
| **test-api-after-docker.ps1** | Comprueba que la API responde tras levantar el stack con `docker compose up` (GET `/api/teams`). |

## Ejemplos

```powershell
# Dejar solo los contenedores necesarios (db, api, frontend) y levantar de nuevo
.\scripts\docker-clean-and-up.ps1

# Subir a GitHub (sustituir URL por la de tu repo)
.\scripts\push-to-github.ps1 -GitHubUrl "https://github.com/usuario/repo.git"

# Tests + build (sin arrancar API)
.\scripts\run-and-test.ps1 -SkipApi

# Tests + build + comprobar API en localhost:5000
.\scripts\run-and-test.ps1

# Tras docker compose up -d
.\scripts\test-api-after-docker.ps1
```
