'use client';

import { useEffect, useState } from 'react';
import { fetchApi, fetchPaged, postApi, putApi, deleteApi } from '@/lib/api';
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
type Player = { id: string; teamId: string; firstName: string; lastName: string; jerseyNumber: string | null; position: string | null; teamName: string; createdAtUtc: string };

export default function PlayersPage() {
  const [paged, setPaged] = useState<Paged<Player> | null>(null);
  const [teams, setTeams] = useState<Team[]>([]);
  const [page, setPage] = useState(1);
  const [teamId, setTeamId] = useState<string>('__all__');
  const [name, setName] = useState('');
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);
  const [form, setForm] = useState({ teamId: '', firstName: '', lastName: '', jerseyNumber: '', position: '' });
  const [editing, setEditing] = useState<Player | null>(null);
  const [popupOpen, setPopupOpen] = useState(false);

  const loadTeams = async () => {
    const r = await fetchPaged<Team>('/api/teams', { pageSize: 100 });
    setTeams(r.data);
    if (!form.teamId && r.data[0]) setForm(f => ({ ...f, teamId: r.data[0].id }));
  };

  const load = async () => {
    setLoading(true);
    setError(null);
    try {
      const r = await fetchPaged<Player>('/api/players', {
        pageNumber: page,
        pageSize: 10,
        teamId: teamId === '__all__' ? undefined : teamId,
        name: name || undefined,
      });
      setPaged(r);
    } catch (e) {
      setError(e instanceof Error ? e.message : 'Error al cargar');
    } finally {
      setLoading(false);
    }
  };

  useEffect(() => { loadTeams(); }, []);
  useEffect(() => { load(); }, [page, teamId]);
  useEffect(() => { const t = setTimeout(() => load(), 300); return () => clearTimeout(t); }, [name]);

  const handleCreate = async (e: React.FormEvent) => {
    e.preventDefault();
    setError(null);
    try {
      await postApi('/api/players', {
        teamId: form.teamId,
        firstName: form.firstName,
        lastName: form.lastName,
        jerseyNumber: form.jerseyNumber || null,
        position: form.position || null,
      }, `player-create-${Date.now()}`);
      setForm({ ...form, firstName: '', lastName: '', jerseyNumber: '', position: '' });
      setPopupOpen(false);
      load();
    } catch (e) {
      setError(e instanceof Error ? e.message : 'Error al crear');
    }
  };

  const handleUpdate = async (e: React.FormEvent) => {
    e.preventDefault();
    if (!editing) return;
    setError(null);
    try {
      await putApi(`/api/players/${editing.id}`, {
        teamId: form.teamId,
        firstName: form.firstName,
        lastName: form.lastName,
        jerseyNumber: form.jerseyNumber || null,
        position: form.position || null,
      });
      setEditing(null);
      setPopupOpen(false);
      load();
    } catch (e) {
      setError(e instanceof Error ? e.message : 'Error al actualizar');
    }
  };

  const openCreatePopup = () => {
    setEditing(null);
    setForm({ teamId: teams[0]?.id ?? '', firstName: '', lastName: '', jerseyNumber: '', position: '' });
    setPopupOpen(true);
  };

  const openEditPopup = (p: Player) => {
    setEditing(p);
    setForm({ teamId: p.teamId, firstName: p.firstName, lastName: p.lastName, jerseyNumber: p.jerseyNumber || '', position: p.position || '' });
    setPopupOpen(true);
  };

  const handleDelete = async (id: string) => {
    if (!confirm('¿Eliminar este jugador?')) return;
    setError(null);
    try {
      await deleteApi(`/api/players/${id}`);
      load();
    } catch (e) {
      setError(e instanceof Error ? e.message : 'Error al eliminar');
    }
  };


  return (
    <div className="space-y-8">
      <PageHeader title="Jugadores" description="Registrar y gestionar jugadores por equipo." />

      {error && (
        <div className="page-section">
          <p className="text-sm text-destructive rounded-lg border border-destructive/20 bg-destructive/10 p-4">{error}</p>
        </div>
      )}

      <section className="page-section" aria-label="Filtros">
      <Card>
        <CardHeader>
          <CardTitle className="text-lg">Filtros</CardTitle>
          <CardDescription>Filtrar por equipo o nombre.</CardDescription>
        </CardHeader>
        <CardContent className="flex flex-wrap gap-4">
          <div className="space-y-2 min-w-[200px]">
            <Label>Nombre</Label>
            <Input placeholder="Filtrar por nombre" value={name} onChange={e => setName(e.target.value)} />
          </div>
          <div className="space-y-2 min-w-[200px]">
            <Label>Equipo</Label>
            <Select value={teamId} onValueChange={setTeamId}>
              <SelectTrigger><SelectValue placeholder="Todos los equipos" /></SelectTrigger>
              <SelectContent>
                <SelectItem value="__all__">Todos los equipos</SelectItem>
                {teams.map(t => <SelectItem key={t.id} value={t.id}>{t.name}</SelectItem>)}
              </SelectContent>
            </Select>
          </div>
        </CardContent>
      </Card>
      </section>

      {loading && (
        <div className="page-section flex items-center gap-3 py-8 text-muted-foreground">
          <div className="h-5 w-5 animate-spin rounded-full border-2 border-primary border-t-transparent" />
          <span>Cargando…</span>
        </div>
      )}

      {paged && !loading && (
        <section className="page-section" aria-label="Listado de jugadores">
          <div className="flex justify-end mb-3">
            <Button onClick={openCreatePopup}>Agregar jugador</Button>
          </div>
          <Card>
            <CardContent className="p-0">
              <Table>
                <TableHeader>
                  <TableRow>
                    <TableHead>Nombre</TableHead>
                    <TableHead>Equipo</TableHead>
                    <TableHead>#</TableHead>
                    <TableHead>Posición</TableHead>
                    <TableHead className="w-[180px]">Acciones</TableHead>
                  </TableRow>
                </TableHeader>
                <TableBody>
                  {paged.data.map(p => (
                    <TableRow key={p.id}>
                      <TableCell className="font-medium">{p.firstName} {p.lastName}</TableCell>
                      <TableCell className="text-muted-foreground">{p.teamName}</TableCell>
                      <TableCell>{p.jerseyNumber || '—'}</TableCell>
                      <TableCell>{p.position || '—'}</TableCell>
                      <TableCell>
                        <div className="flex gap-2">
                          <Button variant="outline" size="sm" onClick={() => openEditPopup(p)}>Editar</Button>
                          <Button variant="destructive" size="sm" onClick={() => handleDelete(p.id)}>Eliminar</Button>
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
            <span className="text-sm text-muted-foreground">Página {paged.pageNumber} de {paged.totalPages} ({paged.totalRecords} en total)</span>
            <Button variant="outline" size="sm" disabled={page >= paged.totalPages} onClick={() => setPage(p => p + 1)}>Siguiente</Button>
          </div>
        </section>
      )}

      <Dialog open={popupOpen} onOpenChange={setPopupOpen}>
        <DialogContent className="sm:max-w-md">
          <DialogHeader>
            <DialogTitle>{editing ? 'Editar jugador' : 'Agregar jugador'}</DialogTitle>
            <DialogDescription>
              {editing ? 'Actualiza los datos del jugador.' : 'Registra un nuevo jugador en un equipo.'}
            </DialogDescription>
          </DialogHeader>
          <form onSubmit={editing ? handleUpdate : handleCreate} className="space-y-4">
            <div className="space-y-2">
              <Label>Equipo</Label>
              <Select value={form.teamId} onValueChange={v => setForm(f => ({ ...f, teamId: v }))} required>
                <SelectTrigger><SelectValue placeholder="Seleccionar" /></SelectTrigger>
                <SelectContent>
                  {teams.map(t => <SelectItem key={t.id} value={t.id}>{t.name}</SelectItem>)}
                </SelectContent>
              </Select>
            </div>
            <div className="grid grid-cols-2 gap-4">
              <div className="space-y-2">
                <Label>Nombre</Label>
                <Input value={form.firstName} onChange={e => setForm(f => ({ ...f, firstName: e.target.value }))} required />
              </div>
              <div className="space-y-2">
                <Label>Apellido</Label>
                <Input value={form.lastName} onChange={e => setForm(f => ({ ...f, lastName: e.target.value }))} required />
              </div>
            </div>
            <div className="grid grid-cols-2 gap-4">
              <div className="space-y-2">
                <Label>Dorsal</Label>
                <Input value={form.jerseyNumber} onChange={e => setForm(f => ({ ...f, jerseyNumber: e.target.value }))} placeholder="Opcional" />
              </div>
              <div className="space-y-2">
                <Label>Posición</Label>
                <Input value={form.position} onChange={e => setForm(f => ({ ...f, position: e.target.value }))} placeholder="Opcional" />
              </div>
            </div>
            <DialogFooter>
              <Button type="button" variant="outline" onClick={() => setPopupOpen(false)}>Cancelar</Button>
              <Button type="submit">{editing ? 'Actualizar' : 'Guardar'}</Button>
            </DialogFooter>
          </form>
        </DialogContent>
      </Dialog>
    </div>
  );
}
