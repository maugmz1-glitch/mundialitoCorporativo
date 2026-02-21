'use client';

import { useEffect, useState } from 'react';
import { fetchApi } from '@/lib/api';
import Loading from '../Loading';

type Standing = { rank: number; teamId: string; teamName: string; played: number; won: number; drawn: number; lost: number; goalsFor: number; goalsAgainst: number; goalDifferential: number; points: number; yellowCards: number; redCards: number };
type TopScorer = { playerId: string; playerName: string; teamName: string; goals: number };

export default function StandingsPage() {
  const [standings, setStandings] = useState<Standing[]>([]);
  const [scorers, setScorers] = useState<TopScorer[]>([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);

  useEffect(() => {
    let cancelled = false;
    (async () => {
      setLoading(true);
      setError(null);
      try {
        const [s, sc] = await Promise.all([
          fetchApi<Standing[]>('/api/standings'),
          fetchApi<TopScorer[]>('/api/standings/top-scorers?limit=10'),
        ]);
        if (!cancelled) {
          setStandings(s);
          setScorers(sc);
        }
      } catch (e) {
        if (!cancelled) setError(e instanceof Error ? e.message : 'Error al cargar');
      } finally {
        if (!cancelled) setLoading(false);
      }
    })();
    return () => { cancelled = true; };
  }, []);

  if (loading) return <Loading />;
  if (error) return <p className="error">{error}</p>;

  return (
    <div>
      <h1>Posiciones</h1>
      <div className="table-wrap">
      <table>
        <thead>
          <tr>
            <th>#</th>
            <th>Equipo</th>
            <th>PJ</th>
            <th>G</th>
            <th>E</th>
            <th>P</th>
            <th>GF</th>
            <th>GC</th>
            <th>DG</th>
            <th>Pts</th>
            <th title="Tarjetas amarillas">TA</th>
            <th title="Tarjetas rojas">TR</th>
          </tr>
        </thead>
        <tbody>
          {standings.map(row => (
            <tr key={row.teamId} className={row.rank === 1 ? 'row-leader' : undefined}>
              <td>{row.rank}</td>
              <td>{row.teamName}</td>
              <td>{row.played}</td>
              <td>{row.won}</td>
              <td>{row.drawn}</td>
              <td>{row.lost}</td>
              <td>{row.goalsFor}</td>
              <td>{row.goalsAgainst}</td>
              <td>{row.goalDifferential}</td>
              <td><strong>{row.points}</strong></td>
              <td>{row.yellowCards}</td>
              <td>{row.redCards}</td>
            </tr>
          ))}
        </tbody>
      </table>
      </div>
      <h2 style={{ marginTop: '1.5rem' }}>Goleadores</h2>
      <div className="table-wrap">
      <table>
        <thead>
          <tr>
            <th>Jugador</th>
            <th>Equipo</th>
            <th>Goles</th>
          </tr>
        </thead>
        <tbody>
          {scorers.map(s => (
            <tr key={s.playerId}>
              <td>{s.playerName}</td>
              <td>{s.teamName}</td>
              <td>{s.goals}</td>
            </tr>
          ))}
        </tbody>
      </table>
      </div>
    </div>
  );
}
