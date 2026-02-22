import type { Metadata } from 'next';
import { Suspense } from 'react';
import './globals.css';
import { Providers } from './providers';
import AuthGuard from './AuthGuard';

export const metadata: Metadata = {
  title: 'Mundialito Corporativo',
  description: 'Sistema de gestión de torneos – equipos, jugadores, partidos y posiciones',
};

export default function RootLayout({
  children,
}: {
  children: React.ReactNode;
}) {
  return (
    <html lang="es">
      <body>
        <Providers>
          <Suspense fallback={<div className="main" style={{ padding: '2rem', textAlign: 'center' }}>Cargando…</div>}>
            <AuthGuard>{children}</AuthGuard>
          </Suspense>
        </Providers>
      </body>
    </html>
  );
}
