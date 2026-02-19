'use client';

import { useEffect, useState } from 'react';
import { fetchApi, fetchPaged, postApi, putApi, deleteApi } from '@/lib/api';
import type { Paged } from '@/lib/api';

type Team = { id: string; name: string };
type Player = { id: string; teamId: string; firstName: string; lastName: string; jerseyNumber: string | null; position: string | null; teamName: string; createdAtUtc: string };

export default function PlayersPage() {
  const [paged, setPaged] = useState<Paged<Player> | null>(null);
  const [teams, setTeams] = useState<Team[]>([]);
  const [page, setPage] = useState(1);
  const [teamId, setTeamId] = useState('');
  const [name, setName] = useState('');
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);
  const [form, setForm] = useState({ teamId: '', firstName: '', lastName: '', jerseyNumber: '', position: '' });
  const [editing, setEditing] = useState<Player | null>(null);

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
        teamId: teamId || undefined,
        name: name || undefined,
      });
      setPaged(r);
    } catch (e) {
      setError(e instanceof Error ? e.message : 'Failed to load');
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
      await putApi(`/api/players/${editing.id}`, {
        teamId: form.teamId,
        firstName: form.firstName,
        lastName: form.lastName,
        jerseyNumber: form.jerseyNumber || null,
        position: form.position || null,
      });
      setEditing(null);
      load();
    } catch (e) {
      setError(e instanceof Error ? e.message : 'Update failed');
    }
  };

  const handleDelete = async (id: string) => {
    if (!confirm('Delete this player?')) return;
    setError(null);
    try {
      await deleteApi(`/api/players/${id}`);
      load();
    } catch (e) {
      setError(e instanceof Error ? e.message : 'Delete failed');
    }
  };

  return (
    <div>
      <h1>Players</h1>
      {error && <p className="error">{error}</p>}
      <div className="card">
        <input placeholder="Filter by name" value={name} onChange={e => setName(e.target.value)} />
        <select value={teamId} onChange={e => setTeamId(e.target.value)}>
          <option value="">All teams</option>
          {teams.map(t => <option key={t.id} value={t.id}>{t.name}</option>)}
        </select>
      </div>
      <div className="card">
        <h2>{editing ? 'Edit player' : 'Add player'}</h2>
        <form onSubmit={editing ? handleUpdate : handleCreate}>
          <label>Team</label>
          <select value={form.teamId} onChange={e => setForm(f => ({ ...f, teamId: e.target.value }))} required>
            {teams.map(t => <option key={t.id} value={t.id}>{t.name}</option>)}
          </select>
          <label>First name</label>
          <input value={form.firstName} onChange={e => setForm(f => ({ ...f, firstName: e.target.value }))} required />
          <label>Last name</label>
          <input value={form.lastName} onChange={e => setForm(f => ({ ...f, lastName: e.target.value }))} required />
          <label>Jersey #</label>
          <input value={form.jerseyNumber} onChange={e => setForm(f => ({ ...f, jerseyNumber: e.target.value }))} />
          <label>Position</label>
          <input value={form.position} onChange={e => setForm(f => ({ ...f, position: e.target.value }))} />
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
                <th>Team</th>
                <th>#</th>
                <th>Position</th>
                <th>Actions</th>
              </tr>
            </thead>
            <tbody>
              {paged.data.map(p => (
                <tr key={p.id}>
                  <td>{p.firstName} {p.lastName}</td>
                  <td>{p.teamName}</td>
                  <td>{p.jerseyNumber || '—'}</td>
                  <td>{p.position || '—'}</td>
                  <td>
                    <button className="btn" onClick={() => { setEditing(p); setForm({ teamId: p.teamId, firstName: p.firstName, lastName: p.lastName, jerseyNumber: p.jerseyNumber || '', position: p.position || '' }); }}>Edit</button>
                    <button className="btn btn-danger" onClick={() => handleDelete(p.id)}>Delete</button>
                  </td>
                </tr>
              ))}
            </tbody>
          </table>
          <div className="pagination">
            <button disabled={page <= 1} onClick={() => setPage(p => p - 1)}>Previous</button>
            <span>Page {paged.pageNumber} of {paged.totalPages}</span>
            <button disabled={page >= paged.totalPages} onClick={() => setPage(p => p + 1)}>Next</button>
          </div>
        </>
      )}
    </div>
  );
}
