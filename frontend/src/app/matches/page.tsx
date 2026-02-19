'use client';

import { useEffect, useState } from 'react';
import { fetchPaged, postApi, putApi, patchApi, deleteApi } from '@/lib/api';
import type { Paged } from '@/lib/api';

type Team = { id: string; name: string };
type Match = {
  id: string; homeTeamId: string; awayTeamId: string; scheduledAtUtc: string; venue: string | null;
  status: number; homeScore: number | null; awayScore: number | null;
  homeTeamName: string; awayTeamName: string;
};

const statusLabels: Record<number, string> = { 0: 'Scheduled', 1: 'In progress', 2: 'Completed', 3: 'Postponed', 4: 'Cancelled' };

export default function MatchesPage() {
  const [paged, setPaged] = useState<Paged<Match> | null>(null);
  const [teams, setTeams] = useState<Team[]>([]);
  const [page, setPage] = useState(1);
  const [status, setStatus] = useState<number | ''>('');
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);
  const [form, setForm] = useState({ homeTeamId: '', awayTeamId: '', scheduledAtUtc: '', venue: '' });
  const [resultMatch, setResultMatch] = useState<Match | null>(null);
  const [resultHome, setResultHome] = useState(0);
  const [resultAway, setResultAway] = useState(0);

  const loadTeams = async () => {
    const r = await fetchPaged<Team>('/api/teams', { pageSize: 100 });
    setTeams(r.data);
    if (!form.homeTeamId && r.data[0]) setForm(f => ({ ...f, homeTeamId: r.data[0].id, awayTeamId: r.data[1]?.id ?? r.data[0].id }));
  };

  const load = async () => {
    setLoading(true);
    setError(null);
    try {
      const r = await fetchPaged<Match>('/api/matches', {
        pageNumber: page,
        pageSize: 10,
        status: status === '' ? undefined : status,
      });
      setPaged(r);
    } catch (e) {
      setError(e instanceof Error ? e.message : 'Failed to load');
    } finally {
      setLoading(false);
    }
  };

  useEffect(() => { loadTeams(); }, []);
  useEffect(() => { load(); }, [page, status]);

  const handleCreate = async (e: React.FormEvent) => {
    e.preventDefault();
    setError(null);
    try {
      await postApi('/api/matches', {
        homeTeamId: form.homeTeamId,
        awayTeamId: form.awayTeamId,
        scheduledAtUtc: form.scheduledAtUtc || new Date().toISOString(),
        venue: form.venue || null,
      }, `match-create-${Date.now()}`);
      setForm({ ...form, scheduledAtUtc: '', venue: '' });
      load();
    } catch (e) {
      setError(e instanceof Error ? e.message : 'Create failed');
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
      setError(e instanceof Error ? e.message : 'Set result failed');
    }
  };

  const handleDelete = async (id: string) => {
    if (!confirm('Delete this match?')) return;
    setError(null);
    try {
      await deleteApi(`/api/matches/${id}`);
      load();
    } catch (e) {
      setError(e instanceof Error ? e.message : 'Delete failed');
    }
  };

  return (
    <div>
      <h1>Matches</h1>
      {error && <p className="error">{error}</p>}
      <div className="card">
        <select value={status} onChange={e => setStatus(e.target.value === '' ? '' : Number(e.target.value))}>
          <option value="">All statuses</option>
          {Object.entries(statusLabels).map(([k, v]) => <option key={k} value={k}>{v}</option>)}
        </select>
      </div>
      <div className="card">
        <h2>Create match</h2>
        <form onSubmit={handleCreate}>
          <label>Home team</label>
          <select value={form.homeTeamId} onChange={e => setForm(f => ({ ...f, homeTeamId: e.target.value }))} required>
            {teams.map(t => <option key={t.id} value={t.id}>{t.name}</option>)}
          </select>
          <label>Away team</label>
          <select value={form.awayTeamId} onChange={e => setForm(f => ({ ...f, awayTeamId: e.target.value }))} required>
            {teams.map(t => <option key={t.id} value={t.id}>{t.name}</option>)}
          </select>
          <label>Date (UTC)</label>
          <input type="datetime-local" value={form.scheduledAtUtc} onChange={e => setForm(f => ({ ...f, scheduledAtUtc: e.target.value }))} />
          <label>Venue</label>
          <input value={form.venue} onChange={e => setForm(f => ({ ...f, venue: e.target.value }))} />
          <button type="submit" className="btn btn-primary">Create</button>
        </form>
      </div>
      {resultMatch && (
        <div className="card">
          <h2>Set result: {resultMatch.homeTeamName} vs {resultMatch.awayTeamName}</h2>
          <form onSubmit={handleSetResult}>
            <input type="number" min={0} value={resultHome} onChange={e => setResultHome(parseInt(e.target.value, 10) || 0)} />
            <span> – </span>
            <input type="number" min={0} value={resultAway} onChange={e => setResultAway(parseInt(e.target.value, 10) || 0)} />
            <button type="submit" className="btn btn-primary">Save</button>
            <button type="button" className="btn" onClick={() => setResultMatch(null)}>Cancel</button>
          </form>
        </div>
      )}
      {loading && <p>Loading…</p>}
      {paged && !loading && (
        <>
          <table>
            <thead>
              <tr>
                <th>Home</th>
                <th>Score</th>
                <th>Away</th>
                <th>Date</th>
                <th>Status</th>
                <th>Actions</th>
              </tr>
            </thead>
            <tbody>
              {paged.data.map(m => (
                <tr key={m.id}>
                  <td>{m.homeTeamName}</td>
                  <td>{m.homeScore != null && m.awayScore != null ? `${m.homeScore} – ${m.awayScore}` : '—'}</td>
                  <td>{m.awayTeamName}</td>
                  <td>{new Date(m.scheduledAtUtc).toLocaleString()}</td>
                  <td>{statusLabels[m.status] ?? m.status}</td>
                  <td>
                    {m.status !== 2 && <button className="btn" onClick={() => { setResultMatch(m); setResultHome(0); setResultAway(0); }}>Set result</button>}
                    <button className="btn btn-danger" onClick={() => handleDelete(m.id)}>Delete</button>
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
