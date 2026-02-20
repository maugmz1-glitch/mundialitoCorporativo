'use client';

import Link from 'next/link';
import { usePathname } from 'next/navigation';

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
      </div>
    </nav>
  );
}
