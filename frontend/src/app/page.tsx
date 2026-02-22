'use client';

import { useState, useEffect, useCallback } from 'react';
import Link from 'next/link';
import {
  Card,
  CardContent,
  CardDescription,
  CardHeader,
  CardTitle,
} from '@/components/ui/card';
import {
  Dialog,
  DialogContent,
  DialogDescription,
  DialogHeader,
  DialogTitle,
} from '@/components/ui/dialog';
import { Button } from '@/components/ui/button';
import {
  Table,
  TableBody,
  TableCell,
  TableHead,
  TableHeader,
  TableRow,
} from '@/components/ui/table';
import { fetchPaged, fetchApi } from '@/lib/api';
import type { Paged } from '@/lib/api';

type CategoryKey = 'teams' | 'players' | 'matches' | 'referees' | 'standings';

type Team = { id: string; name: string; logoUrl: string | null };
type Player = { id: string; firstName: string; lastName: string; teamName: string };
type Match = { id: string; homeTeamName: string; awayTeamName: string; homeScore: number | null; awayScore: number | null; status: number };
type Referee = { id: string; firstName: string; lastName: string };
type Standing = { rank: number; teamId: string; teamName: string; played: number; points: number; goalsFor: number; goalsAgainst: number };

const features: { key: CategoryKey; href: string; title: string; description: string; emoji: string }[] = [
  { key: 'teams', href: '/teams', title: 'Equipos', description: 'Crear y gestionar equipos, filtros y orden.', emoji: '⚽' },
  { key: 'players', href: '/players', title: 'Jugadores', description: 'Registrar jugadores por equipo, posiciones y dorsales.', emoji: '👤' },
  { key: 'matches', href: '/matches', title: 'Partidos', description: 'Programar partidos y cargar resultados.', emoji: '🏟️' },
  { key: 'standings', href: '/standings', title: 'Posiciones', description: 'Tabla de posiciones y goleadores.', emoji: '🏆' },
  { key: 'referees', href: '/referees', title: 'Árbitros', description: 'Gestionar árbitros de la liga.', emoji: '👨‍⚖️' },
];

const statusLabels: Record<number, string> = { 0: 'Programado', 1: 'En curso', 2: 'Finalizado', 3: 'Aplazado', 4: 'Cancelado' };

export default function Home() {
  const [openCategory, setOpenCategory] = useState<CategoryKey | null>(null);
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState<string | null>(null);
  const [teamsPreview, setTeamsPreview] = useState<Paged<Team> | null>(null);
  const [playersPreview, setPlayersPreview] = useState<Paged<Player> | null>(null);
  const [matchesPreview, setMatchesPreview] = useState<Paged<Match> | null>(null);
  const [refereesPreview, setRefereesPreview] = useState<Paged<Referee> | null>(null);
  const [standingsPreview, setStandingsPreview] = useState<Standing[] | null>(null);

  const loadPreview = useCallback(async (category: CategoryKey) => {
    setLoading(true);
    setError(null);
    try {
      switch (category) {
        case 'teams':
          setTeamsPreview(await fetchPaged<Team>('/api/teams', { pageNumber: 1, pageSize: 8 }));
          break;
        case 'players':
          setPlayersPreview(await fetchPaged<Player>('/api/players', { pageNumber: 1, pageSize: 8 }));
          break;
        case 'matches':
          setMatchesPreview(await fetchPaged<Match>('/api/matches', { pageNumber: 1, pageSize: 8 }));
          break;
        case 'referees':
          setRefereesPreview(await fetchPaged<Referee>('/api/referees', { pageNumber: 1, pageSize: 8 }));
          break;
        case 'standings':
          setStandingsPreview(await fetchApi<Standing[]>('/api/standings'));
          break;
      }
    } catch (e) {
      setError(e instanceof Error ? e.message : 'Error al cargar');
    } finally {
      setLoading(false);
    }
  }, []);

  useEffect(() => {
    if (openCategory) loadPreview(openCategory);
  }, [openCategory, loadPreview]);

  const feature = openCategory ? features.find((f) => f.key === openCategory) : null;

  return (
    <div className="space-y-10">
      <Card className="border-primary/20 bg-gradient-to-br from-background to-muted/30">
        <CardHeader className="space-y-2 text-center pb-4">
          <CardTitle className="text-2xl font-bold tracking-tight md:text-3xl">Mundialito Corporativo</CardTitle>
          <CardDescription className="text-base leading-relaxed max-w-xl mx-auto">
            Gestiona tu liga: equipos, jugadores, partidos y posiciones en vivo en un solo lugar.
          </CardDescription>
        </CardHeader>
      </Card>

      <div className="grid gap-4 sm:grid-cols-2 lg:grid-cols-5">
        {features.map((f) => (
          <Card
            key={f.href}
            role="button"
            tabIndex={0}
            className="h-full cursor-pointer transition-all hover:border-primary/50 hover:shadow-md focus:outline-none focus:ring-2 focus:ring-primary focus:ring-offset-2"
            onClick={() => setOpenCategory(f.key)}
            onKeyDown={(e) => e.key === 'Enter' && setOpenCategory(f.key)}
          >
            <CardHeader>
              <span className="text-3xl" role="img" aria-hidden>{f.emoji}</span>
              <CardTitle className="text-lg">{f.title}</CardTitle>
              <CardDescription>{f.description}</CardDescription>
            </CardHeader>
          </Card>
        ))}
      </div>

      <Dialog open={!!openCategory} onOpenChange={(open) => !open && setOpenCategory(null)}>
        <DialogContent className="max-h-[85vh] max-w-2xl overflow-hidden flex flex-col" showCloseButton={true}>
          {feature && (
            <>
              <DialogHeader>
                <DialogTitle>{feature.title}</DialogTitle>
                <DialogDescription>Vista previa. Usa &quot;Ver todo&quot; para gestionar y editar.</DialogDescription>
              </DialogHeader>
              <div className="overflow-y-auto flex-1 min-h-0 -mx-1 px-1">
                {loading && (
                  <div className="flex items-center gap-2 text-muted-foreground py-8">
                    <div className="h-4 w-4 animate-spin rounded-full border-2 border-primary border-t-transparent" />
                    <span>Cargando…</span>
                  </div>
                )}
                {error && (
                  <p className="text-sm text-destructive p-3 rounded-md bg-destructive/10">{error}</p>
                )}
                {!loading && openCategory === 'teams' && teamsPreview && (
                  <Table>
                    <TableHeader>
                      <TableRow>
                        <TableHead>Nombre</TableHead>
                        <TableHead>Logo</TableHead>
                      </TableRow>
                    </TableHeader>
                    <TableBody>
                      {teamsPreview.data.map((t) => (
                        <TableRow key={t.id}>
                          <TableCell className="font-medium">{t.name}</TableCell>
                          <TableCell className="text-muted-foreground text-sm">{t.logoUrl ? 'Sí' : '—'}</TableCell>
                        </TableRow>
                      ))}
                    </TableBody>
                  </Table>
                )}
                {!loading && openCategory === 'players' && playersPreview && (
                  <Table>
                    <TableHeader>
                      <TableRow>
                        <TableHead>Jugador</TableHead>
                        <TableHead>Equipo</TableHead>
                      </TableRow>
                    </TableHeader>
                    <TableBody>
                      {playersPreview.data.map((p) => (
                        <TableRow key={p.id}>
                          <TableCell className="font-medium">{p.firstName} {p.lastName}</TableCell>
                          <TableCell className="text-muted-foreground">{p.teamName}</TableCell>
                        </TableRow>
                      ))}
                    </TableBody>
                  </Table>
                )}
                {!loading && openCategory === 'matches' && matchesPreview && (
                  <Table>
                    <TableHeader>
                      <TableRow>
                        <TableHead>Partido</TableHead>
                        <TableHead>Resultado</TableHead>
                        <TableHead>Estado</TableHead>
                      </TableRow>
                    </TableHeader>
                    <TableBody>
                      {matchesPreview.data.map((m) => (
                        <TableRow key={m.id}>
                          <TableCell className="font-medium">{m.homeTeamName} – {m.awayTeamName}</TableCell>
                          <TableCell className="text-muted-foreground">
                            {m.homeScore != null && m.awayScore != null ? `${m.homeScore} - ${m.awayScore}` : '—'}
                          </TableCell>
                          <TableCell className="text-muted-foreground text-sm">{statusLabels[m.status] ?? m.status}</TableCell>
                        </TableRow>
                      ))}
                    </TableBody>
                  </Table>
                )}
                {!loading && openCategory === 'referees' && refereesPreview && (
                  <Table>
                    <TableHeader>
                      <TableRow>
                        <TableHead>Árbitro</TableHead>
                      </TableRow>
                    </TableHeader>
                    <TableBody>
                      {refereesPreview.data.map((r) => (
                        <TableRow key={r.id}>
                          <TableCell className="font-medium">{r.firstName} {r.lastName}</TableCell>
                        </TableRow>
                      ))}
                    </TableBody>
                  </Table>
                )}
                {!loading && openCategory === 'standings' && standingsPreview && (
                  <Table>
                    <TableHeader>
                      <TableRow>
                        <TableHead className="w-10">#</TableHead>
                        <TableHead>Equipo</TableHead>
                        <TableHead className="text-center">PJ</TableHead>
                        <TableHead className="text-center">GF</TableHead>
                        <TableHead className="text-center">GC</TableHead>
                        <TableHead className="text-center font-semibold">Pts</TableHead>
                      </TableRow>
                    </TableHeader>
                    <TableBody>
                      {standingsPreview.map((row) => (
                        <TableRow key={row.teamId} className={row.rank === 1 ? 'bg-primary/5' : undefined}>
                          <TableCell className="font-medium">{row.rank}</TableCell>
                          <TableCell>{row.teamName}</TableCell>
                          <TableCell className="text-center">{row.played}</TableCell>
                          <TableCell className="text-center">{row.goalsFor}</TableCell>
                          <TableCell className="text-center">{row.goalsAgainst}</TableCell>
                          <TableCell className="text-center font-semibold">{row.points}</TableCell>
                        </TableRow>
                      ))}
                    </TableBody>
                  </Table>
                )}
              </div>
              {feature && (
                <div className="pt-4 border-t flex justify-end">
                  <Button asChild>
                    <Link href={feature.href} onClick={() => setOpenCategory(null)}>
                      Ver todo →
                    </Link>
                  </Button>
                </div>
              )}
            </>
          )}
        </DialogContent>
      </Dialog>
    </div>
  );
}
