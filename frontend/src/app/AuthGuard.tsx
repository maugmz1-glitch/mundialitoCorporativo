'use client';

import { usePathname, useRouter } from 'next/navigation';
import { useEffect, useState } from 'react';
import { getAuthToken } from '@/lib/api';
import Nav from './Nav';

const PUBLIC_PATHS = ['/login', '/register'];

export default function AuthGuard({ children }: { children: React.ReactNode }) {
  const pathname = usePathname();
  const router = useRouter();
  const [ready, setReady] = useState(false);

  const isPublic = PUBLIC_PATHS.includes(pathname ?? '');
  const token = typeof window !== 'undefined' ? getAuthToken() : null;

  useEffect(() => {
    setReady(true);
  }, []);

  useEffect(() => {
    if (!ready) return;
    if (isPublic && token) {
      router.replace('/');
      return;
    }
    if (!isPublic && !token) {
      router.replace('/login');
    }
  }, [ready, isPublic, token, router]);

  if (!ready) {
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

  if (!token) {
    return (
      <div className="main" style={{ display: 'flex', justifyContent: 'center', alignItems: 'center', minHeight: '50vh' }}>
        <p style={{ color: 'var(--text-muted)' }}>Redirigiendo a inicio de sesión…</p>
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
