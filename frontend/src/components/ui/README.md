# Componentes UI – shadcn/ui

Esta carpeta contiene los **componentes de shadcn/ui** usados en el proyecto.

## Cómo reconocer que son de shadcn

- **Radix UI**: cada componente se apoya en un primitivo `@radix-ui/react-*` (por ejemplo `Dialog`, `Select`, `Label`).
- **Estilos**: usan la utilidad `cn()` de `@/lib/utils` (clsx + tailwind-merge) y variables CSS del tema (`--primary`, `--background`, etc.) definidas en `app/globals.css`.
- **Variantes**: los que tienen variantes (p. ej. `Button`) usan `class-variance-authority` (`cva`).

## Componentes disponibles

| Archivo   | Uso típico                          |
|----------|--------------------------------------|
| button   | Botones (default, outline, destructive) |
| input    | Campos de texto                      |
| label    | Etiquetas de formulario              |
| card     | Cards (CardHeader, CardContent, etc.) |
| table    | Tablas (TableHeader, TableRow, etc.) |
| select   | Desplegables                         |
| dialog   | Modales / popups                     |

## Añadir más componentes

Si en la raíz del frontend existe `components.json` (configuración de shadcn), puedes añadir componentes con:

```bash
npx shadcn@latest add <nombre-componente>
```

Se instalarán las dependencias necesarias y se creará el archivo en esta carpeta.
