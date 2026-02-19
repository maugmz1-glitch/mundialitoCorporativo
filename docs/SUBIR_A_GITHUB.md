# Subir el proyecto a GitHub

El repositorio ya está inicializado con las ramas estandarizadas. Sigue estos pasos para subirlo a GitHub.

---

## Ramas creadas

| Rama | Uso |
|------|-----|
| **main** | Código en producción. Solo se actualiza por merge desde `release` o hotfix. |
| **development** | Rama de trabajo diario. Aquí se integran las features vía Pull Request. |
| **release** | Preparación de releases. Se hace merge desde `development`, se prueba y luego se fusiona en `main` con tag. |

Detalle del flujo en **docs/GIT_WORKFLOW.md**.

---

## Pasos para subir a GitHub

### 1. Crear el repositorio en GitHub

1. Entra en [github.com](https://github.com) e inicia sesión.
2. Clic en **New repository** (o el **+** → **New repository**).
3. **Repository name:** por ejemplo `MundialitoCorporativo` (o el nombre que prefieras).
4. **Description:** opcional, por ejemplo "Tournament management system - Clean Architecture, CQRS".
5. Elige **Public** o **Private**.
6. **No** marques "Add a README", "Add .gitignore" ni "Choose a license" (ya tienes todo en local).
7. Clic en **Create repository**.

### 2. Conectar tu carpeta local con GitHub y subir

En la raíz del proyecto (donde está `.git`), ejecuta en la terminal, **sustituyendo `TU_USUARIO` y `TU_REPO`** por tu usuario de GitHub y el nombre del repo:

```powershell
cd c:\temp\dotnet\primerApi\MundialitoCorporativo

# Añadir el remoto (usa la URL que te muestra GitHub al crear el repo)
git remote add origin https://github.com/TU_USUARIO/TU_REPO.git

# Subir todas las ramas (main, development, release)
git push -u origin main
git push -u origin development
git push -u origin release
```

Si GitHub te creó el repo con README y quieres mantener el historial local:

```powershell
git remote add origin https://github.com/TU_USUARIO/TU_REPO.git
git pull origin main --allow-unrelated-histories
# Resuelve conflictos si los hay (por ejemplo quedarte con tu README)
git push -u origin main
git push -u origin development
git push -u origin release
```

### 3. Configurar la rama por defecto en GitHub (recomendado)

1. En GitHub: **Settings** del repositorio → **General**.
2. En **Default branch** elige **main** (o **development** si quieres que al clonar salgan en development).
3. **Update** → **I understand, update the default branch**.

---

## Comprobar que todo está subido

- En la pestaña **Code** deberías ver las ramas: **main**, **development**, **release**.
- El contenido de **main** debe coincidir con tu commit inicial (solution, src, frontend, docs, etc.).

---

## Siguientes pasos (flujo de trabajo)

- Trabajo diario: `git checkout development` y crea ramas desde ahí, por ejemplo `feature/nueva-funcionalidad`.
- Integración: Pull Request de `feature/...` → **development**.
- Para publicar: merge **development** → **release**, probar, luego **release** → **main** y crear tag (ej. `v1.0.0`).

Ver **docs/GIT_WORKFLOW.md** para el flujo completo.
