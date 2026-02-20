import Link from 'next/link';

const features = [
  {
    href: '/teams',
    title: 'Equipos',
    description: 'Crear y gestionar equipos, filtros y orden.',
    emoji: '⚽',
  },
  {
    href: '/players',
    title: 'Jugadores',
    description: 'Registrar jugadores por equipo, posiciones y dorsales.',
    emoji: '👤',
  },
  {
    href: '/matches',
    title: 'Partidos',
    description: 'Programar partidos y cargar resultados.',
    emoji: '🏟️',
  },
  {
    href: '/standings',
    title: 'Posiciones',
    description: 'Tabla de posiciones y goleadores.',
    emoji: '🏆',
  },
];

export default function Home() {
  return (
    <div className="home">
      <section className="hero">
        <h1>Mundialito Corporativo</h1>
        <p className="hero-sub">
          Gestiona tu liga: equipos, jugadores, partidos y posiciones en vivo en un solo lugar.
        </p>
      </section>

      <section className="features">
        {features.map((f) => (
          <Link key={f.href} href={f.href} className="feature-card">
            <span className="feature-emoji">{f.emoji}</span>
            <h2>{f.title}</h2>
            <p>{f.description}</p>
          </Link>
        ))}
      </section>
    </div>
  );
}
