import type { Metadata } from 'next';
import './globals.css';

export const metadata: Metadata = {
  title: 'Mundialito Tournament',
  description: 'Tournament management system',
};

export default function RootLayout({
  children,
}: {
  children: React.ReactNode;
}) {
  return (
    <html lang="en">
      <body>
        <nav className="nav">
          <a href="/">Home</a>
          <a href="/teams">Teams</a>
          <a href="/players">Players</a>
          <a href="/matches">Matches</a>
          <a href="/standings">Standings</a>
        </nav>
        <main className="main">{children}</main>
      </body>
    </html>
  );
}
