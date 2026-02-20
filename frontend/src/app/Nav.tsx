'use client';

import Link from 'next/link';
import { usePathname, useRouter } from 'next/navigation';
import { useEffect, useState } from 'react';
import { getAuthUser, clearAuth } from '@/lib/api';

const links = [
  { href: '/', label: 'Inicio' },
  { href: '/teams', label: 'Equipos' },
  { href: '/players', label: 'Jugadores' },
  { href: '/matches', label: 'Partidos' },
  { href: '/standings', label: 'Posiciones' },
];

export default function Nav() {
  const pathname = usePathname();
  const router = useRouter();
  const [user, setUser] = useState<string | null>(null);

  useEffect(() => { setUser(getAuthUser()); }, [pathname]);

  const handleLogout = () => {
    clearAuth();
    setUser(null);
    router.push('/');
    router.refresh();
  };

  return (
    <nav className="nav">
      <Link href="/" className="nav-brand">
        Mundialito<span>.</span>
      </Link>
      <div className="nav-links">
        {links.map(({ href, label }) => (
          <Link
            key={href}
            href={href}
            className={pathname === href ? 'active' : undefined}
          >
            {label}
          </Link>
        ))}
        {user ? (
          <span className="nav-user">
            <span style={{ marginRight: '0.5rem' }}>{user}</span>
            <button type="button" className="btn btn-small" onClick={handleLogout}>Cerrar sesión</button>
          </span>
        ) : (
          <>
            <Link href="/register" className={pathname === '/register' ? 'active' : undefined}>Crear cuenta</Link>
            <Link href="/login" className={pathname === '/login' ? 'active' : undefined}>Iniciar sesión</Link>
          </>
        )}
      </div>
    </nav>
  );
}
