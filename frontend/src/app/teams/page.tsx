'use client';

import { useEffect, useState } from 'react';
import { fetchPaged, postApi, putApi, deleteApi } from '@/lib/api';
import type { Paged } from '@/lib/api';

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
      setError(e instanceof Error ? e.message : 'Failed to load');
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
      setError(e instanceof Error ? e.message : 'Create failed');
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
      setError(e instanceof Error ? e.message : 'Update failed');
    }
  };

  const handleDelete = async (id: string) => {
    if (!confirm('Delete this team?')) return;
    setError(null);
    try {
      await deleteApi(`/api/teams/${id}`);
      load();
    } catch (e) {
      setError(e instanceof Error ? e.message : 'Delete failed');
    }
  };

  const startEdit = (t: Team) => {
    setEditing(t);
    setFormName(t.name);
    setFormLogo(t.logoUrl || '');
  };

  return (
    <div>
      <h1>Teams</h1>
      {error && <p className="error">{error}</p>}
      <div className="card">
        <h2>Filters</h2>
        <input placeholder="Filter by name" value={name} onChange={e => setName(e.target.value)} />
        <select value={sortBy} onChange={e => setSortBy(e.target.value)}>
          <option value="name">Name</option>
          <option value="createdAtUtc">Created</option>
        </select>
        <select value={sortDir} onChange={e => setSortDir(e.target.value)}>
          <option value="asc">Asc</option>
          <option value="desc">Desc</option>
        </select>
      </div>
      <div className="card">
        <h2>{editing ? 'Edit team' : 'Create team'}</h2>
        <form onSubmit={editing ? handleUpdate : handleCreate}>
          <label>Name</label>
          <input value={formName} onChange={e => setFormName(e.target.value)} required />
          <label>Logo URL</label>
          <input value={formLogo} onChange={e => setFormLogo(e.target.value)} />
          <button type="submit" className="btn btn-primary">{editing ? 'Update' : 'Create'}</button>
          {editing && <button type="button" className="btn" onClick={() => setEditing(null)}>Cancel</button>}
        </form>
      </div>
      {loading && <p>Loading…</p>}
      {paged && !loading && (
        <>
          <table>
            <thead>
              <tr>
                <th>Name</th>
                <th>Logo</th>
                <th>Actions</th>
              </tr>
            </thead>
            <tbody>
              {paged.data.map(t => (
                <tr key={t.id}>
                  <td>{t.name}</td>
                  <td>{t.logoUrl || '—'}</td>
                  <td>
                    <button className="btn" onClick={() => startEdit(t)}>Edit</button>
                    <button className="btn btn-danger" onClick={() => handleDelete(t.id)}>Delete</button>
                  </td>
                </tr>
              ))}
            </tbody>
          </table>
          <div className="pagination">
            <button disabled={page <= 1} onClick={() => setPage(p => p - 1)}>Previous</button>
            <span>Page {paged.pageNumber} of {paged.totalPages} ({paged.totalRecords} total)</span>
            <button disabled={page >= paged.totalPages} onClick={() => setPage(p => p + 1)}>Next</button>
          </div>
        </>
      )}
    </div>
  );
}
