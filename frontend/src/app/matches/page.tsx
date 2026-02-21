'use client';

import { useEffect, useState } from 'react';
import { fetchApi, fetchPaged, postApi, patchApi, deleteApi } from '@/lib/api';
import type { Paged } from '@/lib/api';
import Loading from '../Loading';

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
  const [status, setStatus] = useState<number | ''>('');
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);
  const [form, setForm] = useState({ homeTeamId: '', awayTeamId: '', refereeId: '', scheduledAtUtc: '', venue: '' });
  const [resultMatch, setResultMatch] = useState<Match | null>(null);
  const [resultHome, setResultHome] = useState(0);
  const [resultAway, setResultAway] = useState(0);
  const [cardMatch, setCardMatch] = useState<MatchDetail | null>(null);
  const [matchPlayers, setMatchPlayers] = useState<Player[]>([]);
  const [cardForm, setCardForm] = useState({ playerId: '', cardType: 0, minute: 0 });

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
        status: status === '' ? undefined : status,
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
    <div>
      <h1>Partidos</h1>
      {error && <p className="error">{error}</p>}
      <div className="card">
        <select value={status} onChange={e => setStatus(e.target.value === '' ? '' : Number(e.target.value))}>
          <option value="">Todos los estados</option>
          {Object.entries(statusLabels).map(([k, v]) => <option key={k} value={k}>{v}</option>)}
        </select>
      </div>
      <div className="card">
        <h2>Crear partido</h2>
        <form onSubmit={handleCreate}>
          <label>Equipo local</label>
          <select value={form.homeTeamId} onChange={e => setForm(f => ({ ...f, homeTeamId: e.target.value }))} required>
            {teams.map(t => <option key={t.id} value={t.id}>{t.name}</option>)}
          </select>
          <label>Equipo visitante</label>
          <select value={form.awayTeamId} onChange={e => setForm(f => ({ ...f, awayTeamId: e.target.value }))} required>
            {teams.map(t => <option key={t.id} value={t.id}>{t.name}</option>)}
          </select>
          <label>Árbitro</label>
          <select value={form.refereeId} onChange={e => setForm(f => ({ ...f, refereeId: e.target.value }))}>
            <option value="">Sin asignar</option>
            {referees.map(r => <option key={r.id} value={r.id}>{r.firstName} {r.lastName}</option>)}
          </select>
          <label>Fecha y hora (UTC)</label>
          <input type="datetime-local" value={form.scheduledAtUtc} onChange={e => setForm(f => ({ ...f, scheduledAtUtc: e.target.value }))} />
          <label>Sede</label>
          <input value={form.venue} onChange={e => setForm(f => ({ ...f, venue: e.target.value }))} />
          <button type="submit" className="btn btn-primary">Crear</button>
        </form>
      </div>
      {resultMatch && (
        <div className="card">
          <h2>Cargar resultado: {resultMatch.homeTeamName} vs {resultMatch.awayTeamName}</h2>
          <form onSubmit={handleSetResult}>
            <input type="number" min={0} value={resultHome} onChange={e => setResultHome(parseInt(e.target.value, 10) || 0)} />
            <span> – </span>
            <input type="number" min={0} value={resultAway} onChange={e => setResultAway(parseInt(e.target.value, 10) || 0)} />
            <button type="submit" className="btn btn-primary">Guardar</button>
            <button type="button" className="btn" onClick={() => setResultMatch(null)}>Cancelar</button>
          </form>
        </div>
      )}
      {cardMatch && (
        <div className="card">
          <h2>Tarjetas del partido: {cardMatch.homeTeamName} vs {cardMatch.awayTeamName}</h2>
          {cardMatch.cards.length > 0 && (
            <div className="table-wrap" style={{ marginBottom: '1rem' }}>
              <table>
                <thead>
                  <tr><th>Jugador</th><th>Tipo</th><th>Min</th></tr>
                </thead>
                <tbody>
                  {cardMatch.cards.map(c => (
                    <tr key={c.id}>
                      <td>{c.playerName}</td>
                      <td>{cardTypeLabels[c.cardType] ?? c.cardType}</td>
                      <td>{c.minute}</td>
                    </tr>
                  ))}
                </tbody>
              </table>
            </div>
          )}
          <form onSubmit={handleAddCard}>
            <label>Jugador</label>
            <select value={cardForm.playerId} onChange={e => setCardForm(f => ({ ...f, playerId: e.target.value }))} required disabled={matchPlayers.length === 0}>
              {matchPlayers.length === 0 && <option value="">Sin jugadores en los equipos</option>}
              {matchPlayers.map(p => (
                <option key={p.id} value={p.id}>{p.firstName} {p.lastName} ({p.teamName})</option>
              ))}
            </select>
            <label>Tipo</label>
            <select value={cardForm.cardType} onChange={e => setCardForm(f => ({ ...f, cardType: Number(e.target.value) }))}>
              {Object.entries(cardTypeLabels).map(([k, v]) => <option key={k} value={k}>{v}</option>)}
            </select>
            <label>Minuto</label>
            <input type="number" min={0} max={999} value={cardForm.minute || ''} onChange={e => setCardForm(f => ({ ...f, minute: parseInt(e.target.value, 10) || 0 }))} />
            <button type="submit" className="btn btn-primary" disabled={!cardForm.playerId || matchPlayers.length === 0}>Registrar tarjeta</button>
            <button type="button" className="btn" onClick={() => setCardMatch(null)}>Cerrar</button>
          </form>
        </div>
      )}
      {loading && <Loading />}
      {paged && !loading && (
        <>
          <div className="table-wrap">
          <table>
            <thead>
              <tr>
                <th>Local</th>
                <th>Resultado</th>
                <th>Visitante</th>
                <th>Árbitro</th>
                <th>Fecha</th>
                <th>Estado</th>
                <th>Acciones</th>
              </tr>
            </thead>
            <tbody>
              {paged.data.map(m => (
                <tr key={m.id}>
                  <td>{m.homeTeamName}</td>
                  <td>{m.homeScore != null && m.awayScore != null ? `${m.homeScore} – ${m.awayScore}` : '—'}</td>
                  <td>{m.awayTeamName}</td>
                  <td>{m.refereeName ?? '—'}</td>
                  <td>{new Date(m.scheduledAtUtc).toLocaleString()}</td>
                  <td>{statusLabels[m.status] ?? m.status}</td>
                  <td>
                    <button className="btn" onClick={() => openCardPanel(m)}>Tarjetas</button>
                    {m.status !== 2 && <button className="btn" onClick={() => { setResultMatch(m); setResultHome(0); setResultAway(0); }}>Cargar resultado</button>}
                    <button className="btn btn-danger" onClick={() => handleDelete(m.id)}>Eliminar</button>
                  </td>
                </tr>
              ))}
            </tbody>
          </table>
          </div>
          <div className="pagination">
            <button disabled={page <= 1} onClick={() => setPage(p => p - 1)}>Anterior</button>
            <span>Página {paged.pageNumber} de {paged.totalPages}</span>
            <button disabled={page >= paged.totalPages} onClick={() => setPage(p => p + 1)}>Siguiente</button>
          </div>
        </>
      )}
    </div>
  );
}
