# Frontend – Mundialito Corporativo

Aplicación Next.js 14 que consume la API del backend. **La interfaz está construida con [shadcn/ui](https://ui.shadcn.com/).**

## Cómo saber que usa shadcn/ui

- **Componentes:** `src/components/ui/` — Button, Input, Label, Card, Table, Select, Dialog (basados en Radix UI + Tailwind + CVA).
- **Configuración:** `components.json` en la raíz del frontend (schema de shadcn, estilo `new-york`).
- **Dependencias:** En `package.json`, `@radix-ui/react-*`, `class-variance-authority`, `clsx`, `tailwind-merge`, `tailwindcss-animate`, `lucide-react`.

Ver también la sección **"Cómo saber que shadcn/ui está aplicado"** en [COMO-LEER-EL-PROYECTO.md](../COMO-LEER-EL-PROYECTO.md) (raíz del repo).

## Stack

| Tecnología | Uso |
|------------|-----|
| Next.js 14 | App Router, SSR, API routes (proxy + NextAuth) |
| React 18 | UI |
| TypeScript | Tipado |
| **shadcn/ui** | Componentes (Radix + Tailwind + CVA) |
| Tailwind CSS | Estilos |
| NextAuth | Sesión (JWT en cookie, proxy con backend) |

## Comandos

```bash
npm install
npm run dev    # http://localhost:3000
npm run build
npm run start
```
