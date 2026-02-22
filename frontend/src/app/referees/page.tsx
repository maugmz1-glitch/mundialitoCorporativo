'use client';

import { useEffect, useState } from 'react';
import { fetchPaged, postApi, putApi, deleteApi } from '@/lib/api';
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

type Referee = { id: string; firstName: string; lastName: string; licenseNumber: string | null; createdAtUtc: string };

export default function RefereesPage() {
  const [paged, setPaged] = useState<Paged<Referee> | null>(null);
  const [page, setPage] = useState(1);
  const [name, setName] = useState('');
  const [sortBy, setSortBy] = useState('firstName');
  const [sortDir, setSortDir] = useState('asc');
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);
  const [editing, setEditing] = useState<Referee | null>(null);
  const [formFirstName, setFormFirstName] = useState('');
  const [formLastName, setFormLastName] = useState('');
  const [formLicense, setFormLicense] = useState('');
  const [popupOpen, setPopupOpen] = useState(false);

  const load = async () => {
    setLoading(true);
    setError(null);
    try {
      const r = await fetchPaged<Referee>('/api/referees', {
        pageNumber: page,
        pageSize: 10,
        sortBy,
        sortDirection: sortDir,
        name: name || undefined,
      });
      setPaged(r);
    } catch (e) {
      setError(e instanceof Error ? e.message : 'Error al cargar');
    } finally {
      setLoading(false);
    }
  };

  useEffect(() => { load(); }, [page, sortBy, sortDir]);
  useEffect(() => { const t = setTimeout(() => load(), 300); return () => clearTimeout(t); }, [name]);

  const handleCreate = async (e: React.FormEvent) => {
    e.preventDefault();
    setError(null);
    try {
      await postApi<Referee>('/api/referees', {
        firstName: formFirstName,
        lastName: formLastName,
        licenseNumber: formLicense || null,
      }, `referee-create-${Date.now()}`);
      setFormFirstName(''); setFormLastName(''); setFormLicense('');
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
      await putApi<Referee>(`/api/referees/${editing.id}`, {
        firstName: formFirstName,
        lastName: formLastName,
        licenseNumber: formLicense || null,
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
    setFormFirstName('');
    setFormLastName('');
    setFormLicense('');
    setPopupOpen(true);
  };

  const openEditPopup = (r: Referee) => {
    setEditing(r);
    setFormFirstName(r.firstName);
    setFormLastName(r.lastName);
    setFormLicense(r.licenseNumber || '');
    setPopupOpen(true);
  };

  const handleDelete = async (id: string) => {
    if (!confirm('¿Eliminar este árbitro?')) return;
    setError(null);
    try {
      await deleteApi(`/api/referees/${id}`);
      load();
    } catch (e) {
      setError(e instanceof Error ? e.message : 'Error al eliminar');
    }
  };


  return (
    <div className="space-y-8">
      <PageHeader title="Árbitros" description="Gestionar árbitros de la liga." />

      {error && (
        <div className="page-section">
          <p className="text-sm text-destructive rounded-lg border border-destructive/20 bg-destructive/10 p-4">{error}</p>
        </div>
      )}

      <section className="page-section" aria-label="Filtros">
      <Card>
        <CardHeader>
          <CardTitle className="text-lg">Filtros</CardTitle>
          <CardDescription>Buscar y ordenar árbitros.</CardDescription>
        </CardHeader>
        <CardContent className="flex flex-wrap gap-4">
          <div className="space-y-2 min-w-[200px]">
            <Label>Nombre</Label>
            <Input placeholder="Filtrar por nombre" value={name} onChange={e => setName(e.target.value)} />
          </div>
          <div className="space-y-2 min-w-[180px]">
            <Label>Ordenar por</Label>
            <Select value={sortBy} onValueChange={setSortBy}>
              <SelectTrigger><SelectValue /></SelectTrigger>
              <SelectContent>
                <SelectItem value="firstName">Nombre</SelectItem>
                <SelectItem value="lastName">Apellido</SelectItem>
                <SelectItem value="createdAtUtc">Fecha de creación</SelectItem>
              </SelectContent>
            </Select>
          </div>
          <div className="space-y-2 min-w-[120px]">
            <Label>Orden</Label>
            <Select value={sortDir} onValueChange={setSortDir}>
              <SelectTrigger><SelectValue /></SelectTrigger>
              <SelectContent>
                <SelectItem value="asc">Asc</SelectItem>
                <SelectItem value="desc">Desc</SelectItem>
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
        <section className="page-section" aria-label="Listado de árbitros">
          <div className="flex justify-end mb-3">
            <Button onClick={openCreatePopup}>Nuevo árbitro</Button>
          </div>
          <Card>
            <CardContent className="p-0">
              <Table>
                <TableHeader>
                  <TableRow>
                    <TableHead>Nombre</TableHead>
                    <TableHead>Apellido</TableHead>
                    <TableHead>Licencia</TableHead>
                    <TableHead className="w-[180px]">Acciones</TableHead>
                  </TableRow>
                </TableHeader>
                <TableBody>
                  {paged.data.map(r => (
                    <TableRow key={r.id}>
                      <TableCell className="font-medium">{r.firstName}</TableCell>
                      <TableCell>{r.lastName}</TableCell>
                      <TableCell className="text-muted-foreground">{r.licenseNumber || '—'}</TableCell>
                      <TableCell>
                        <div className="flex gap-2">
                          <Button variant="outline" size="sm" onClick={() => openEditPopup(r)}>Editar</Button>
                          <Button variant="destructive" size="sm" onClick={() => handleDelete(r.id)}>Eliminar</Button>
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
            <DialogTitle>{editing ? 'Editar árbitro' : 'Nuevo árbitro'}</DialogTitle>
            <DialogDescription>
              {editing ? 'Actualiza los datos del árbitro.' : 'Registra un nuevo árbitro.'}
            </DialogDescription>
          </DialogHeader>
          <form onSubmit={editing ? handleUpdate : handleCreate} className="space-y-4">
            <div className="space-y-2">
              <Label>Nombre</Label>
              <Input value={formFirstName} onChange={e => setFormFirstName(e.target.value)} required />
            </div>
            <div className="space-y-2">
              <Label>Apellido</Label>
              <Input value={formLastName} onChange={e => setFormLastName(e.target.value)} required />
            </div>
            <div className="space-y-2">
              <Label>Nº de licencia</Label>
              <Input value={formLicense} onChange={e => setFormLicense(e.target.value)} placeholder="Opcional" />
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
