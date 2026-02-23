'use client';

import { useEffect, useState } from 'react';
import { fetchApi } from '@/lib/api';
import {
  Card,
  CardContent,
  CardDescription,
  CardHeader,
  CardTitle,
} from '@/components/ui/card';
import {
  Table,
  TableBody,
  TableCell,
  TableHead,
  TableHeader,
  TableRow,
} from '@/components/ui/table';
import { PageHeader } from '@/components/PageHeader';

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

  if (loading) {
    return (
      <div className="flex items-center gap-3 py-12 text-muted-foreground">
        <div className="h-5 w-5 animate-spin rounded-full border-2 border-primary border-t-transparent" />
        <span>Cargando…</span>
      </div>
    );
  }
  if (error) {
    return (
      <div className="space-y-8">
        <PageHeader title="Posiciones" description="Tabla de posiciones y goleadores." />
        <div className="page-section">
          <p className="text-destructive rounded-lg border border-destructive/20 bg-destructive/10 p-4">{error}</p>
        </div>
      </div>
    );
  }

  return (
    <div className="space-y-8">
      <PageHeader title="Posiciones" description="Tabla de posiciones y goleadores." />

      <section className="page-section" aria-label="Tabla de posiciones">
      <Card>
        <CardHeader>
          <CardTitle className="text-lg">Tabla de posiciones</CardTitle>
          <CardDescription>PJ: partidos jugados, G/E/P: ganados/empatados/perdidos, GF/GC: goles a favor/en contra, DG: diferencia de goles.</CardDescription>
        </CardHeader>
        <CardContent className="p-0">
          <Table>
            <TableHeader>
              <TableRow>
                <TableHead className="w-12">#</TableHead>
                <TableHead>Equipo</TableHead>
                <TableHead className="text-center">PJ</TableHead>
                <TableHead className="text-center">G</TableHead>
                <TableHead className="text-center">E</TableHead>
                <TableHead className="text-center">P</TableHead>
                <TableHead className="text-center">GF</TableHead>
                <TableHead className="text-center">GC</TableHead>
                <TableHead className="text-center">DG</TableHead>
                <TableHead className="text-center font-semibold">Pts</TableHead>
                <TableHead className="text-center" title="Tarjetas amarillas">TA</TableHead>
                <TableHead className="text-center" title="Tarjetas rojas">TR</TableHead>
              </TableRow>
            </TableHeader>
            <TableBody>
              {standings.map(row => (
                <TableRow key={row.teamId} className={row.rank === 1 ? 'bg-primary/5 border-l-4 border-l-primary' : undefined}>
                  <TableCell className="font-medium">{row.rank}</TableCell>
                  <TableCell className="font-medium">{row.teamName}</TableCell>
                  <TableCell className="text-center">{row.played}</TableCell>
                  <TableCell className="text-center">{row.won}</TableCell>
                  <TableCell className="text-center">{row.drawn}</TableCell>
                  <TableCell className="text-center">{row.lost}</TableCell>
                  <TableCell className="text-center">{row.goalsFor}</TableCell>
                  <TableCell className="text-center">{row.goalsAgainst}</TableCell>
                  <TableCell className="text-center">{row.goalDifferential}</TableCell>
                  <TableCell className="text-center font-semibold">{row.points}</TableCell>
                  <TableCell className="text-center">{row.yellowCards}</TableCell>
                  <TableCell className="text-center">{row.redCards}</TableCell>
                </TableRow>
              ))}
            </TableBody>
          </Table>
        </CardContent>
      </Card>
      </section>

      <section className="page-section" aria-label="Goleadores">
      <Card>
        <CardHeader>
          <CardTitle className="text-lg">Goleadores</CardTitle>
          <CardDescription>Top 10 anotadores.</CardDescription>
        </CardHeader>
        <CardContent className="p-0">
          <Table>
            <TableHeader>
              <TableRow>
                <TableHead>Jugador</TableHead>
                <TableHead>Equipo</TableHead>
                <TableHead className="text-right">Goles</TableHead>
              </TableRow>
            </TableHeader>
            <TableBody>
              {scorers.map(s => (
                <TableRow key={s.playerId}>
                  <TableCell className="font-medium">{s.playerName}</TableCell>
                  <TableCell className="text-muted-foreground">{s.teamName}</TableCell>
                  <TableCell className="text-right font-semibold">{s.goals}</TableCell>
                </TableRow>
              ))}
            </TableBody>
          </Table>
        </CardContent>
      </Card>
      </section>
    </div>
  );
}
