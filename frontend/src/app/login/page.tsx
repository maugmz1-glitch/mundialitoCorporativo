'use client';

import { Suspense, useState, useEffect } from 'react';
import { useRouter, useSearchParams } from 'next/navigation';
import Link from 'next/link';
import { setAuth } from '@/lib/api';

function LoginForm() {
  const router = useRouter();
  const searchParams = useSearchParams();
  const [username, setUsername] = useState('');
  const [password, setPassword] = useState('');
  const [error, setError] = useState<string | null>(null);
  const [success, setSuccess] = useState<string | null>(null);
  const [loading, setLoading] = useState(false);

  useEffect(() => {
    if (searchParams.get('registered') === '1') setSuccess('Cuenta creada. Ya puedes iniciar sesión.');
  }, [searchParams]);

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    setError(null);
    setLoading(true);
    try {
      const res = await fetch('/api/auth/login', {
        method: 'POST',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify({ username, password }),
      });
      if (!res.ok) {
        const data = await res.json().catch(() => ({}));
        throw new Error((data as { message?: string }).message || 'Usuario o contraseña incorrectos.');
      }
      const data = await res.json() as { token: string; userName: string };
      setAuth(data.token, data.userName);
      router.push('/');
      router.refresh();
    } catch (e) {
      const msg = e instanceof Error ? e.message : 'Error al iniciar sesión.';
      if (msg === 'fetch failed' || msg === 'Failed to fetch' || msg.includes('NetworkError')) {
        setError('No se pudo conectar con el servidor. Comprueba que la API y el frontend estén en ejecución (p. ej. docker compose up).');
      } else {
        setError(msg);
      }
    } finally {
      setLoading(false);
    }
  };

  return (
    <div className="card" style={{ maxWidth: 400, margin: '2rem auto' }}>
      <h1>Iniciar sesión</h1>
      <p style={{ color: 'var(--text-muted)', marginBottom: '1rem' }}>
        Ingresa con tu usuario y contraseña. Si no tienes cuenta, créala primero.
      </p>
      {success && <p className="success" style={{ color: 'var(--success, green)', marginBottom: '0.5rem' }}>{success}</p>}
      {error && <p className="error">{error}</p>}
      <form onSubmit={handleSubmit}>
        <label>Usuario</label>
        <input
          type="text"
          value={username}
          onChange={e => setUsername(e.target.value)}
          required
          autoComplete="username"
        />
        <label>Contraseña</label>
        <input
          type="password"
          value={password}
          onChange={e => setPassword(e.target.value)}
          required
          autoComplete="current-password"
        />
        <button type="submit" className="btn btn-primary" disabled={loading}>
          {loading ? 'Entrando…' : 'Entrar'}
        </button>
      </form>
      <p style={{ marginTop: '1rem', fontSize: '0.9rem', color: 'var(--text-muted)' }}>
        ¿No tienes cuenta? <Link href="/register">Crear cuenta</Link>
      </p>
      <p style={{ marginTop: '0.5rem', fontSize: '0.85rem', color: 'var(--text-muted)' }}>
        Usuario demo: <strong>demo</strong> / <strong>Demo123!</strong>
      </p>
    </div>
  );
}

export default function LoginPage() {
  return (
    <Suspense fallback={<div className="card" style={{ maxWidth: 400, margin: '2rem auto' }}>Cargando…</div>}>
      <LoginForm />
    </Suspense>
  );
}
