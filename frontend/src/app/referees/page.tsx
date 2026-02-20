'use client';

import { useEffect, useState } from 'react';
import { fetchPaged, postApi, putApi, deleteApi } from '@/lib/api';
import type { Paged } from '@/lib/api';
import Loading from '../Loading';

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
      load();
    } catch (e) {
      setError(e instanceof Error ? e.message : 'Error al actualizar');
    }
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

  const startEdit = (r: Referee) => {
    setEditing(r);
    setFormFirstName(r.firstName);
    setFormLastName(r.lastName);
    setFormLicense(r.licenseNumber || '');
  };

  return (
    <div>
      <h1>Árbitros</h1>
      {error && <p className="error">{error}</p>}
      <div className="card">
        <h2>Filtros</h2>
        <div style={{ display: 'flex', gap: '1rem', flexWrap: 'wrap', alignItems: 'flex-end' }}>
          <div><label>Nombre</label><input placeholder="Filtrar por nombre" value={name} onChange={e => setName(e.target.value)} /></div>
          <div><label>Ordenar por</label><select value={sortBy} onChange={e => setSortBy(e.target.value)}>
            <option value="firstName">Nombre</option>
            <option value="lastName">Apellido</option>
            <option value="createdAtUtc">Fecha de creación</option>
          </select></div>
          <div><label>Orden</label><select value={sortDir} onChange={e => setSortDir(e.target.value)}>
            <option value="asc">Asc</option>
            <option value="desc">Desc</option>
          </select></div>
        </div>
      </div>
      <div className="card">
        <h2>{editing ? 'Editar árbitro' : 'Nuevo árbitro'}</h2>
        <form onSubmit={editing ? handleUpdate : handleCreate}>
          <label>Nombre</label>
          <input value={formFirstName} onChange={e => setFormFirstName(e.target.value)} required />
          <label>Apellido</label>
          <input value={formLastName} onChange={e => setFormLastName(e.target.value)} required />
          <label>Nº de licencia</label>
          <input value={formLicense} onChange={e => setFormLicense(e.target.value)} placeholder="Opcional" />
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
                  <th>Apellido</th>
                  <th>Licencia</th>
                  <th>Acciones</th>
                </tr>
              </thead>
              <tbody>
                {paged.data.map(r => (
                  <tr key={r.id}>
                    <td>{r.firstName}</td>
                    <td>{r.lastName}</td>
                    <td>{r.licenseNumber || '—'}</td>
                    <td>
                      <button className="btn" onClick={() => startEdit(r)}>Editar</button>
                      <button className="btn btn-danger" onClick={() => handleDelete(r.id)}>Eliminar</button>
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
