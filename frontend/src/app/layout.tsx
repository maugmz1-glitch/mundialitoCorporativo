import type { Metadata } from 'next';
import './globals.css';
import Nav from './Nav';

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
        <header className="top-bar">
          <span>Mundialito Corporativo</span>
          <span className="top-bar-sep">|</span>
          <span>Equipos · Jugadores · Partidos · Posiciones</span>
        </header>
        <Nav />
        <main className="main">{children}</main>
      </body>
    </html>
  );
}
