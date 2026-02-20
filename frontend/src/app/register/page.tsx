'use client';

import { useState } from 'react';
import { useRouter } from 'next/navigation';
import Link from 'next/link';

export default function RegisterPage() {
  const router = useRouter();
  const [userName, setUserName] = useState('');
  const [email, setEmail] = useState('');
  const [password, setPassword] = useState('');
  const [confirmPassword, setConfirmPassword] = useState('');
  const [error, setError] = useState<string | null>(null);
  const [loading, setLoading] = useState(false);

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    setError(null);
    if (password !== confirmPassword) {
      setError('Las contraseñas no coinciden.');
      return;
    }
    if (password.length < 6) {
      setError('La contraseña debe tener al menos 6 caracteres.');
      return;
    }
    if (userName.trim().length < 3) {
      setError('El nombre de usuario debe tener al menos 3 caracteres.');
      return;
    }
    setLoading(true);
    try {
      const res = await fetch('/api/auth/register', {
        method: 'POST',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify({
          userName: userName.trim(),
          email: email.trim() || null,
          password,
        }),
      });
      const data = await res.json().catch(() => ({})) as { message?: string };
      if (!res.ok) {
        throw new Error(data.message || 'Error al crear la cuenta.');
      }
      router.push('/login?registered=1');
      router.refresh();
    } catch (e) {
      setError(e instanceof Error ? e.message : 'Error al crear la cuenta.');
    } finally {
      setLoading(false);
    }
  };

  return (
    <div className="card" style={{ maxWidth: 400, margin: '2rem auto' }}>
      <h1>Crear cuenta</h1>
      <p style={{ color: 'var(--text-muted)', marginBottom: '1rem' }}>
        Regístrate para poder ingresar al sistema.
      </p>
      {error && <p className="error">{error}</p>}
      <form onSubmit={handleSubmit}>
        <label>Nombre de usuario *</label>
        <input
          type="text"
          value={userName}
          onChange={e => setUserName(e.target.value)}
          required
          minLength={3}
          placeholder="Mínimo 3 caracteres"
          autoComplete="username"
        />
        <label>Correo (opcional)</label>
        <input
          type="email"
          value={email}
          onChange={e => setEmail(e.target.value)}
          placeholder="tu@correo.com"
          autoComplete="email"
        />
        <label>Contraseña *</label>
        <input
          type="password"
          value={password}
          onChange={e => setPassword(e.target.value)}
          required
          minLength={6}
          placeholder="Mínimo 6 caracteres"
          autoComplete="new-password"
        />
        <label>Confirmar contraseña *</label>
        <input
          type="password"
          value={confirmPassword}
          onChange={e => setConfirmPassword(e.target.value)}
          required
          placeholder="Repite la contraseña"
          autoComplete="new-password"
        />
        <button type="submit" className="btn btn-primary" disabled={loading}>
          {loading ? 'Creando cuenta…' : 'Crear cuenta'}
        </button>
      </form>
      <p style={{ marginTop: '1rem', fontSize: '0.9rem', color: 'var(--text-muted)' }}>
        ¿Ya tienes cuenta? <Link href="/login">Iniciar sesión</Link>
      </p>
    </div>
  );
}
