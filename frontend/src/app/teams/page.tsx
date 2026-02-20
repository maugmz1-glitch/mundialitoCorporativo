'use client';

import { useEffect, useState } from 'react';
import { fetchPaged, postApi, putApi, deleteApi } from '@/lib/api';
import type { Paged } from '@/lib/api';
import Loading from '../Loading';

type Team = { id: string; name: string; logoUrl: string | null; createdAtUtc: string };

export default function TeamsPage() {
  const [paged, setPaged] = useState<Paged<Team> | null>(null);
  const [page, setPage] = useState(1);
  const [name, setName] = useState('');
  const [sortBy, setSortBy] = useState('name');
  const [sortDir, setSortDir] = useState('asc');
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);
  const [editing, setEditing] = useState<Team | null>(null);
  const [formName, setFormName] = useState('');
  const [formLogo, setFormLogo] = useState('');

  const load = async () => {
    setLoading(true);
    setError(null);
    try {
      const r = await fetchPaged<Team>('/api/teams', {
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
      await postApi<Team>('/api/teams', { name: formName, logoUrl: formLogo || null }, `team-create-${Date.now()}`);
      setFormName(''); setFormLogo('');
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
      await putApi<Team>(`/api/teams/${editing.id}`, { name: formName, logoUrl: formLogo || null });
      setEditing(null);
      load();
    } catch (e) {
      setError(e instanceof Error ? e.message : 'Error al actualizar');
    }
  };

  const handleDelete = async (id: string) => {
    if (!confirm('¿Eliminar este equipo?')) return;
    setError(null);
    try {
      await deleteApi(`/api/teams/${id}`);
      load();
    } catch (e) {
      setError(e instanceof Error ? e.message : 'Error al eliminar');
    }
  };

  const startEdit = (t: Team) => {
    setEditing(t);
    setFormName(t.name);
    setFormLogo(t.logoUrl || '');
  };

  return (
    <div>
      <h1>Equipos</h1>
      {error && <p className="error">{error}</p>}
      <div className="card">
        <h2>Filtros</h2>
        <div style={{ display: 'flex', gap: '1rem', flexWrap: 'wrap', alignItems: 'flex-end' }}>
        <div><label>Nombre</label><input placeholder="Filtrar por nombre" value={name} onChange={e => setName(e.target.value)} /></div>
        <div><label>Ordenar por</label><select value={sortBy} onChange={e => setSortBy(e.target.value)}>
          <option value="name">Nombre</option>
          <option value="createdAtUtc">Fecha de creación</option>
        </select></div>
        <div><label>Orden</label><select value={sortDir} onChange={e => setSortDir(e.target.value)}>
          <option value="asc">Asc</option>
          <option value="desc">Desc</option>
        </select></div>
        </div>
      </div>
      <div className="card">
        <h2>{editing ? 'Editar equipo' : 'Crear equipo'}</h2>
        <form onSubmit={editing ? handleUpdate : handleCreate}>
          <label>Nombre</label>
          <input value={formName} onChange={e => setFormName(e.target.value)} required />
          <label>URL del logo</label>
          <input value={formLogo} onChange={e => setFormLogo(e.target.value)} />
          <button type="submit" className="btn btn-primary">{editing ? 'Actualizar' : 'Crear'}</button>
          {editing && <button type="button" className="btn" onClick={() => setEditing(null)}>Cancelar</button>}
        </form>
      </div>
      {loading && <Loading />}
      {paged && !loading && (
        <>
          <div className="table-wrap">
          <table>
            <thead>
              <tr>
                <th>Nombre</th>
                <th>Logo</th>
                <th>Acciones</th>
              </tr>
            </thead>
            <tbody>
              {paged.data.map(t => (
                <tr key={t.id}>
                  <td>{t.name}</td>
                  <td>{t.logoUrl || '—'}</td>
                  <td>
                    <button className="btn" onClick={() => startEdit(t)}>Editar</button>
                    <button className="btn btn-danger" onClick={() => handleDelete(t.id)}>Eliminar</button>
                  </td>
                </tr>
              ))}
            </tbody>
          </table>
          </div>
          <div className="pagination">
            <button disabled={page <= 1} onClick={() => setPage(p => p - 1)}>Anterior</button>
            <span>Página {paged.pageNumber} de {paged.totalPages} ({paged.totalRecords} en total)</span>
            <button disabled={page >= paged.totalPages} onClick={() => setPage(p => p + 1)}>Siguiente</button>
          </div>
        </>
      )}
    </div>
  );
}
