'use client';

import { useEffect, useState } from 'react';
import { fetchApi } from '@/lib/api';

type Standing = { rank: number; teamId: string; teamName: string; played: number; won: number; drawn: number; lost: number; goalsFor: number; goalsAgainst: number; goalDifferential: number; points: number };
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
        if (!cancelled) setError(e instanceof Error ? e.message : 'Failed to load');
      } finally {
        if (!cancelled) setLoading(false);
      }
    })();
    return () => { cancelled = true; };
  }, []);

  if (loading) return <p>Loading…</p>;
  if (error) return <p className="error">{error}</p>;

  return (
    <div>
      <h1>Standings</h1>
      <table>
        <thead>
          <tr>
            <th>#</th>
            <th>Team</th>
            <th>P</th>
            <th>W</th>
            <th>D</th>
            <th>L</th>
            <th>GF</th>
            <th>GA</th>
            <th>GD</th>
            <th>Pts</th>
          </tr>
        </thead>
        <tbody>
          {standings.map(row => (
            <tr key={row.teamId}>
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
            </tr>
          ))}
        </tbody>
      </table>
      <h2>Top scorers</h2>
      <table>
        <thead>
          <tr>
            <th>Player</th>
            <th>Team</th>
            <th>Goals</th>
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
  );
}
