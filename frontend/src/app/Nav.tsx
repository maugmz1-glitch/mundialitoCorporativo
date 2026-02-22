'use client';

import { useState } from 'react';
import Link from 'next/link';
import { usePathname } from 'next/navigation';
import { useSession, signOut } from 'next-auth/react';
import { Button } from '@/components/ui/button';

const links = [
  { href: '/', label: 'Inicio' },
  { href: '/teams', label: 'Equipos' },
  { href: '/players', label: 'Jugadores' },
  { href: '/matches', label: 'Partidos' },
  { href: '/referees', label: 'Árbitros' },
  { href: '/standings', label: 'Posiciones' },
];

export default function Nav() {
  const pathname = usePathname();
  const { data: session } = useSession();
  const user = session?.user?.name ?? null;
  const [logoutError, setLogoutError] = useState<string | null>(null);
  const [logoutLoading, setLogoutLoading] = useState(false);

  const handleLogout = async () => {
    setLogoutError(null);
    setLogoutLoading(true);
    try {
      // redirect: false evita que NextAuth redirija a 0.0.0.0 (host de escucha), que el navegador no puede abrir.
      await signOut({ redirect: false });
      // Redirigir con la misma origen (localhost o la que use el usuario) para que la sesión quede limpia.
      window.location.href = '/login?loggedout=1';
    } catch (e) {
      const msg = e instanceof Error ? e.message : 'Error al cerrar sesión.';
      setLogoutError(msg);
      setLogoutLoading(false);
    }
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
            {logoutError && (
              <span className="text-white/90 text-sm mr-2" title={logoutError}>
                Error al salir
              </span>
            )}
            <span className="mr-2 text-white font-medium">{user}</span>
            <Button
              type="button"
              variant="outline"
              size="sm"
              className="border-white/50 bg-transparent text-white hover:bg-white/20 hover:text-white hover:border-white/70"
              onClick={handleLogout}
              disabled={logoutLoading}
            >
              {logoutLoading ? 'Cerrando…' : 'Cerrar sesión'}
            </Button>
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
