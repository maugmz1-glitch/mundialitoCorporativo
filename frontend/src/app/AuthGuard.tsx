'use client';

import { usePathname, useRouter } from 'next/navigation';
import { useEffect, useState } from 'react';
import { useSession } from 'next-auth/react';
import Nav from './Nav';

const PUBLIC_PATHS = ['/login', '/register'];

export default function AuthGuard({ children }: { children: React.ReactNode }) {
  const pathname = usePathname();
  const router = useRouter();
  const { data: session, status } = useSession();
  const [ready, setReady] = useState(false);

  const isPublic = PUBLIC_PATHS.includes(pathname ?? '');
  const hasValidSession = status === 'authenticated' && !!session?.user?.name;
  const isAuthenticated = hasValidSession;

  useEffect(() => {
    setReady(true);
  }, []);

  useEffect(() => {
    if (!ready || status === 'loading') return;
    if (pathname === '/register' && hasValidSession) {
      router.replace('/');
      return;
    }
    if (!isPublic && !hasValidSession) {
      router.replace('/login');
    }
  }, [ready, status, pathname, isPublic, hasValidSession, router]);

  if (!ready || status === 'loading') {
    return (
      <div className="main" style={{ display: 'flex', justifyContent: 'center', alignItems: 'center', minHeight: '50vh' }}>
        <p style={{ color: 'var(--text-muted)' }}>Cargando…</p>
      </div>
    );
  }

  if (isPublic) {
    return (
      <>
        <header className="top-bar" style={{ justifyContent: 'center' }}>
          <span>Mundialito Corporativo</span>
        </header>
        <main className="main">{children}</main>
      </>
    );
  }

  if (!isAuthenticated) {
    return (
      <div className="main" style={{ display: 'flex', flexDirection: 'column', justifyContent: 'center', alignItems: 'center', minHeight: '50vh', gap: '0.5rem' }}>
        <p style={{ color: 'var(--text-muted)' }}>Sesión no válida o expirada.</p>
        <p style={{ color: 'var(--text-muted)', fontSize: '0.9rem' }}>Redirigiendo a inicio de sesión…</p>
      </div>
    );
  }

  return (
    <>
      <header className="top-bar">
        <span>Mundialito Corporativo</span>
        <span className="top-bar-sep">|</span>
        <span>Equipos · Jugadores · Partidos · Posiciones</span>
      </header>
      <Nav />
      <main className="main">{children}</main>
    </>
  );
}
