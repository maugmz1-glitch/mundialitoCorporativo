'use client';

import { useEffect, useState } from 'react';
import { fetchApi, fetchPaged, postApi, patchApi, deleteApi } from '@/lib/api';
import type { Paged } from '@/lib/api';
import { Button } from '@/components/ui/button';
import { Input } from '@/components/ui/input';
import { Label } from '@/components/ui/label';
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
import {
  Select,
  SelectContent,
  SelectItem,
  SelectTrigger,
  SelectValue,
} from '@/components/ui/select';
import {
  Dialog,
  DialogContent,
  DialogDescription,
  DialogFooter,
  DialogHeader,
  DialogTitle,
} from '@/components/ui/dialog';
import { PageHeader } from '@/components/PageHeader';

type Team = { id: string; name: string };
type Referee = { id: string; firstName: string; lastName: string };
type Player = { id: string; teamId: string; firstName: string; lastName: string; teamName: string };
type MatchCard = { id: string; playerId: string; playerName: string; cardType: number; minute: number };
type MatchDetail = {
  id: string; homeTeamId: string; awayTeamId: string; refereeId: string | null; refereeName: string | null;
  scheduledAtUtc: string; venue: string | null; status: number; homeScore: number | null; awayScore: number | null;
  homeTeamName: string; awayTeamName: string; cards: MatchCard[];
};
type Match = {
  id: string; homeTeamId: string; awayTeamId: string; refereeId: string | null; refereeName: string | null;
  scheduledAtUtc: string; venue: string | null;
  status: number; homeScore: number | null; awayScore: number | null;
  homeTeamName: string; awayTeamName: string;
};

const statusLabels: Record<number, string> = { 0: 'Programado', 1: 'En curso', 2: 'Finalizado', 3: 'Aplazado', 4: 'Cancelado' };
const cardTypeLabels: Record<number, string> = { 0: 'Amarilla', 1: 'Roja' };

export default function MatchesPage() {
  const [paged, setPaged] = useState<Paged<Match> | null>(null);
  const [teams, setTeams] = useState<Team[]>([]);
  const [referees, setReferees] = useState<Referee[]>([]);
  const [page, setPage] = useState(1);
  const [status, setStatus] = useState<string>('__all__');
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);
  const [form, setForm] = useState({ homeTeamId: '', awayTeamId: '', refereeId: '', scheduledAtUtc: '', venue: '' });
  const [resultMatch, setResultMatch] = useState<Match | null>(null);
  const [resultHome, setResultHome] = useState(0);
  const [resultAway, setResultAway] = useState(0);
  const [cardMatch, setCardMatch] = useState<MatchDetail | null>(null);
  const [matchPlayers, setMatchPlayers] = useState<Player[]>([]);
  const [cardForm, setCardForm] = useState({ playerId: '', cardType: 0, minute: 0 });
  const [createPopupOpen, setCreatePopupOpen] = useState(false);

  const loadTeams = async () => {
    const r = await fetchPaged<Team>('/api/teams', { pageSize: 100 });
    setTeams(r.data);
    if (!form.homeTeamId && r.data[0]) setForm(f => ({ ...f, homeTeamId: r.data[0].id, awayTeamId: r.data[1]?.id ?? r.data[0].id }));
  };
  const loadReferees = async () => {
    const r = await fetchPaged<Referee>('/api/referees', { pageSize: 100 });
    setReferees(r.data);
  };

  const load = async () => {
    setLoading(true);
    setError(null);
    try {
      const r = await fetchPaged<Match>('/api/matches', {
        pageNumber: page,
        pageSize: 10,
        status: status === '__all__' ? undefined : Number(status),
      });
      setPaged(r);
    } catch (e) {
      setError(e instanceof Error ? e.message : 'Error al cargar');
    } finally {
      setLoading(false);
    }
  };

  useEffect(() => { loadTeams(); loadReferees(); }, []);
  useEffect(() => { load(); }, [page, status]);

  const openCardPanel = async (m: Match) => {
    setError(null);
    setCardMatch(null);
    setMatchPlayers([]);
    setCardForm({ playerId: '', cardType: 0, minute: 0 });
    try {
      const detail = await fetchApi<MatchDetail>(`/api/matches/${m.id}`);
      setCardMatch(detail);
      const [homePl, awayPl] = await Promise.all([
        fetchPaged<Player>('/api/players', { teamId: m.homeTeamId, pageSize: 50 }),
        fetchPaged<Player>('/api/players', { teamId: m.awayTeamId, pageSize: 50 }),
      ]);
      setMatchPlayers([...homePl.data.map(p => ({ ...p, teamName: m.homeTeamName })), ...awayPl.data.map(p => ({ ...p, teamName: m.awayTeamName }))]);
      if (homePl.data[0] || awayPl.data[0]) setCardForm(f => ({ ...f, playerId: homePl.data[0]?.id ?? awayPl.data[0]?.id ?? '' }));
    } catch (e) {
      setError(e instanceof Error ? e.message : 'Error al cargar partido');
    }
  };

  const handleAddCard = async (e: React.FormEvent) => {
    e.preventDefault();
    if (!cardMatch || !cardForm.playerId) return;
    setError(null);
    try {
      await postApi(`/api/matches/${cardMatch.id}/cards`, {
        playerId: cardForm.playerId,
        cardType: cardForm.cardType,
        minute: cardForm.minute,
      }, `match-card-${cardMatch.id}-${Date.now()}`);
      const updated = await fetchApi<MatchDetail>(`/api/matches/${cardMatch.id}`);
      setCardMatch(updated);
      setCardForm(f => ({ ...f, minute: 0 }));
    } catch (e) {
      setError(e instanceof Error ? e.message : 'Error al registrar tarjeta');
    }
  };

  const handleCreate = async (e: React.FormEvent) => {
    e.preventDefault();
    setError(null);
    try {
      await postApi('/api/matches', {
        homeTeamId: form.homeTeamId,
        awayTeamId: form.awayTeamId,
        ...(form.refereeId ? { refereeId: form.refereeId } : {}),
        scheduledAtUtc: form.scheduledAtUtc || new Date().toISOString(),
        venue: form.venue || null,
      }, `match-create-${Date.now()}`);
      setForm({ ...form, scheduledAtUtc: '', venue: '' });
      setCreatePopupOpen(false);
      load();
    } catch (e) {
      setError(e instanceof Error ? e.message : 'Error al crear');
    }
  };

  const handleSetResult = async (e: React.FormEvent) => {
    e.preventDefault();
    if (!resultMatch) return;
    setError(null);
    try {
      await patchApi(`/api/matches/${resultMatch.id}/result`, { homeScore: resultHome, awayScore: resultAway });
      setResultMatch(null);
      load();
    } catch (e) {
      setError(e instanceof Error ? e.message : 'Error al guardar resultado');
    }
  };

  const handleDelete = async (id: string) => {
    if (!confirm('¿Eliminar este partido?')) return;
    setError(null);
    try {
      await deleteApi(`/api/matches/${id}`);
      load();
    } catch (e) {
      setError(e instanceof Error ? e.message : 'Error al eliminar');
    }
  };

  return (
    <div className="space-y-8">
      <PageHeader title="Partidos" description="Programar partidos, cargar resultados y tarjetas." />

      {error && (
        <div className="page-section">
          <p className="text-sm text-destructive rounded-lg border border-destructive/20 bg-destructive/10 p-4">{error}</p>
        </div>
      )}

      <section className="page-section" aria-label="Filtros">
      <Card>
        <CardHeader>
          <CardTitle className="text-lg">Filtro</CardTitle>
          <CardDescription>Filtrar por estado del partido.</CardDescription>
        </CardHeader>
        <CardContent>
          <div className="flex flex-wrap gap-4 items-end">
            <div className="space-y-2 min-w-[180px]">
              <Label>Estado</Label>
              <Select value={status} onValueChange={setStatus}>
                <SelectTrigger><SelectValue placeholder="Todos" /></SelectTrigger>
                <SelectContent>
                  <SelectItem value="__all__">Todos los estados</SelectItem>
                  {Object.entries(statusLabels).map(([k, v]) => <SelectItem key={k} value={k}>{v}</SelectItem>)}
                </SelectContent>
              </Select>
            </div>
          </div>
        </CardContent>
      </Card>
      </section>

      {resultMatch && (
        <Card>
          <CardHeader>
            <CardTitle className="text-base">Cargar resultado: {resultMatch.homeTeamName} vs {resultMatch.awayTeamName}</CardTitle>
          </CardHeader>
          <CardContent>
            <form onSubmit={handleSetResult} className="flex flex-wrap gap-4 items-end">
              <div className="flex items-center gap-2">
                <Input type="number" min={0} className="w-20" value={resultHome} onChange={e => setResultHome(parseInt(e.target.value, 10) || 0)} />
                <span>–</span>
                <Input type="number" min={0} className="w-20" value={resultAway} onChange={e => setResultAway(parseInt(e.target.value, 10) || 0)} />
              </div>
              <Button type="submit">Guardar</Button>
              <Button type="button" variant="outline" onClick={() => setResultMatch(null)}>Cancelar</Button>
            </form>
          </CardContent>
        </Card>
      )}

      {cardMatch && (
        <Card>
          <CardHeader>
            <CardTitle className="text-base">Tarjetas: {cardMatch.homeTeamName} vs {cardMatch.awayTeamName}</CardTitle>
          </CardHeader>
          <CardContent className="space-y-4">
            {cardMatch.cards.length > 0 && (
              <Table>
                <TableHeader>
                  <TableRow><TableHead>Jugador</TableHead><TableHead>Tipo</TableHead><TableHead>Min</TableHead></TableRow>
                </TableHeader>
                <TableBody>
                  {cardMatch.cards.map(c => (
                    <TableRow key={c.id}>
                      <TableCell>{c.playerName}</TableCell>
                      <TableCell>{cardTypeLabels[c.cardType] ?? c.cardType}</TableCell>
                      <TableCell>{c.minute}</TableCell>
                    </TableRow>
                  ))}
                </TableBody>
              </Table>
            )}
            <form onSubmit={handleAddCard} className="flex flex-wrap gap-4 items-end">
              <div className="space-y-2 min-w-[200px]">
                <Label>Jugador</Label>
                <Select value={cardForm.playerId} onValueChange={v => setCardForm(f => ({ ...f, playerId: v }))} disabled={matchPlayers.length === 0}>
                  <SelectTrigger><SelectValue placeholder={matchPlayers.length === 0 ? 'Sin jugadores' : 'Seleccionar'} /></SelectTrigger>
                  <SelectContent>
                    {matchPlayers.map(p => <SelectItem key={p.id} value={p.id}>{p.firstName} {p.lastName} ({p.teamName})</SelectItem>)}
                  </SelectContent>
                </Select>
              </div>
              <div className="space-y-2 min-w-[120px]">
                <Label>Tipo</Label>
                <Select value={String(cardForm.cardType)} onValueChange={v => setCardForm(f => ({ ...f, cardType: Number(v) }))}>
                  <SelectTrigger><SelectValue /></SelectTrigger>
                  <SelectContent>
                    {Object.entries(cardTypeLabels).map(([k, v]) => <SelectItem key={k} value={k}>{v}</SelectItem>)}
                  </SelectContent>
                </Select>
              </div>
              <div className="space-y-2 min-w-[100px]">
                <Label>Minuto</Label>
                <Input type="number" min={0} max={999} value={cardForm.minute || ''} onChange={e => setCardForm(f => ({ ...f, minute: parseInt(e.target.value, 10) || 0 }))} />
              </div>
              <Button type="submit" disabled={!cardForm.playerId || matchPlayers.length === 0}>Registrar tarjeta</Button>
              <Button type="button" variant="outline" onClick={() => setCardMatch(null)}>Cerrar</Button>
            </form>
          </CardContent>
        </Card>
      )}

      {loading && (
        <div className="page-section flex items-center gap-3 py-8 text-muted-foreground">
          <div className="h-5 w-5 animate-spin rounded-full border-2 border-primary border-t-transparent" />
          <span>Cargando…</span>
        </div>
      )}

      {paged && !loading && (
        <section className="page-section" aria-label="Listado de partidos">
          <div className="flex justify-end mb-3">
            <Button onClick={() => setCreatePopupOpen(true)}>Crear partido</Button>
          </div>
          <Card>
            <CardContent className="p-0">
              <Table>
                <TableHeader>
                  <TableRow>
                    <TableHead>Local</TableHead>
                    <TableHead>Resultado</TableHead>
                    <TableHead>Visitante</TableHead>
                    <TableHead>Árbitro</TableHead>
                    <TableHead>Fecha</TableHead>
                    <TableHead>Estado</TableHead>
                    <TableHead className="w-[260px]">Acciones</TableHead>
                  </TableRow>
                </TableHeader>
                <TableBody>
                  {paged.data.map(m => (
                    <TableRow key={m.id}>
                      <TableCell className="font-medium">{m.homeTeamName}</TableCell>
                      <TableCell>{m.homeScore != null && m.awayScore != null ? `${m.homeScore} – ${m.awayScore}` : '—'}</TableCell>
                      <TableCell className="font-medium">{m.awayTeamName}</TableCell>
                      <TableCell className="text-muted-foreground">{m.refereeName ?? '—'}</TableCell>
                      <TableCell className="text-muted-foreground text-sm">{new Date(m.scheduledAtUtc).toLocaleString()}</TableCell>
                      <TableCell>{statusLabels[m.status] ?? m.status}</TableCell>
                      <TableCell>
                        <div className="flex gap-2 flex-wrap">
                          <Button variant="outline" size="sm" onClick={() => openCardPanel(m)}>Tarjetas</Button>
                          {m.status !== 2 && <Button variant="outline" size="sm" onClick={() => { setResultMatch(m); setResultHome(0); setResultAway(0); }}>Resultado</Button>}
                          <Button variant="destructive" size="sm" onClick={() => handleDelete(m.id)}>Eliminar</Button>
                        </div>
                      </TableCell>
                    </TableRow>
                  ))}
                </TableBody>
              </Table>
            </CardContent>
          </Card>
          <div className="flex flex-wrap items-center gap-4 border-t bg-muted/30 px-4 py-3">
            <Button variant="outline" size="sm" disabled={page <= 1} onClick={() => setPage(p => p - 1)}>Anterior</Button>
            <span className="text-sm text-muted-foreground">Página {paged.pageNumber} de {paged.totalPages}</span>
            <Button variant="outline" size="sm" disabled={page >= paged.totalPages} onClick={() => setPage(p => p + 1)}>Siguiente</Button>
          </div>
        </section>
      )}

      <Dialog open={createPopupOpen} onOpenChange={setCreatePopupOpen}>
        <DialogContent className="sm:max-w-lg">
          <DialogHeader>
            <DialogTitle>Crear partido</DialogTitle>
            <DialogDescription>Programa un nuevo partido entre dos equipos.</DialogDescription>
          </DialogHeader>
          <form onSubmit={handleCreate} className="space-y-4">
            <div className="grid grid-cols-2 gap-4">
              <div className="space-y-2">
                <Label>Equipo local</Label>
                <Select value={form.homeTeamId} onValueChange={v => setForm(f => ({ ...f, homeTeamId: v }))} required>
                  <SelectTrigger><SelectValue placeholder="Seleccionar" /></SelectTrigger>
                  <SelectContent>
                    {teams.map(t => <SelectItem key={t.id} value={t.id}>{t.name}</SelectItem>)}
                  </SelectContent>
                </Select>
              </div>
              <div className="space-y-2">
                <Label>Equipo visitante</Label>
                <Select value={form.awayTeamId} onValueChange={v => setForm(f => ({ ...f, awayTeamId: v }))} required>
                  <SelectTrigger><SelectValue placeholder="Seleccionar" /></SelectTrigger>
                  <SelectContent>
                    {teams.map(t => <SelectItem key={t.id} value={t.id}>{t.name}</SelectItem>)}
                  </SelectContent>
                </Select>
              </div>
            </div>
            <div className="space-y-2">
              <Label>Árbitro</Label>
              <Select value={form.refereeId || '__none__'} onValueChange={v => setForm(f => ({ ...f, refereeId: v === '__none__' ? '' : v }))}>
                <SelectTrigger><SelectValue placeholder="Sin asignar" /></SelectTrigger>
                <SelectContent>
                  <SelectItem value="__none__">Sin asignar</SelectItem>
                  {referees.map(r => <SelectItem key={r.id} value={r.id}>{r.firstName} {r.lastName}</SelectItem>)}
                </SelectContent>
              </Select>
            </div>
            <div className="grid grid-cols-2 gap-4">
              <div className="space-y-2">
                <Label>Fecha y hora (UTC)</Label>
                <Input type="datetime-local" value={form.scheduledAtUtc} onChange={e => setForm(f => ({ ...f, scheduledAtUtc: e.target.value }))} />
              </div>
              <div className="space-y-2">
                <Label>Sede</Label>
                <Input value={form.venue} onChange={e => setForm(f => ({ ...f, venue: e.target.value }))} placeholder="Opcional" />
              </div>
            </div>
            <DialogFooter>
              <Button type="button" variant="outline" onClick={() => setCreatePopupOpen(false)}>Cancelar</Button>
              <Button type="submit">Guardar</Button>
            </DialogFooter>
          </form>
        </DialogContent>
      </Dialog>
    </div>
  );
}
