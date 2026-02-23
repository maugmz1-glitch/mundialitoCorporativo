'use client';

import { Suspense, useState, useEffect } from 'react';
import { useSearchParams } from 'next/navigation';
import Link from 'next/link';
import { signIn } from 'next-auth/react';
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

function LoginForm() {
  const searchParams = useSearchParams();
  const [username, setUsername] = useState('');
  const [password, setPassword] = useState('');
  const [error, setError] = useState<string | null>(null);
  const [success, setSuccess] = useState<string | null>(null);
  const [loading, setLoading] = useState(false);

  useEffect(() => {
    if (searchParams.get('registered') === '1') {
      setSuccess('Cuenta creada. Ya puedes iniciar sesión.');
      setError(null);
    } else if (searchParams.get('loggedout') === '1') {
      setSuccess('Sesión cerrada correctamente.');
      setError(null);
    } else if (searchParams.get('error') === 'session_expired') {
      setError('Tu sesión ha expirado. Vuelve a iniciar sesión.');
      setSuccess(null);
    }
  }, [searchParams]);

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    setError(null);
    setSuccess(null);

    const user = username.trim();
    if (!user) {
      setError('El usuario es obligatorio.');
      return;
    }
    if (user.length < 3) {
      setError('El usuario debe tener al menos 3 caracteres.');
      return;
    }
    if (!password) {
      setError('La contraseña es obligatoria.');
      return;
    }
    if (password.length < 6) {
      setError('La contraseña debe tener al menos 6 caracteres.');
      return;
    }

    setLoading(true);
    try {
      const result = await signIn('credentials', {
        username: user,
        password,
        redirect: false,
      });
      if (result?.error) {
        setError(result.error === 'CredentialsSignin' ? 'Usuario o contraseña incorrectos.' : result.error);
        setLoading(false);
        return;
      }
      if (result?.ok) {
        setSuccess('Sesión iniciada. Redirigiendo…');
        window.location.href = '/';
        return;
      }
      setError('No se pudo iniciar sesión. Intenta de nuevo.');
    } catch (e) {
      const msg = e instanceof Error ? e.message : 'Error al iniciar sesión.';
      if (msg === 'fetch failed' || msg === 'Failed to fetch' || msg.includes('NetworkError')) {
        setError('No se pudo conectar con el servidor. Comprueba que la API esté en ejecución (puerto 5000 o docker compose up).');
      } else {
        setError(msg);
      }
    } finally {
      setLoading(false);
    }
  };

  return (
    <div className="mx-auto max-w-[420px] px-4 py-10">
      <Card>
        <CardHeader className="space-y-1.5">
          <CardTitle className="text-xl">Iniciar sesión</CardTitle>
          <CardDescription className="text-base leading-relaxed">
            Ingresa con tu usuario y contraseña. Si no tienes cuenta, créala primero.
          </CardDescription>
        </CardHeader>
        <CardContent className="space-y-4">
        {success && (
          <p className="rounded-lg border border-primary/20 bg-primary/10 p-4 text-sm text-primary">
            {success}
          </p>
        )}
        {error && (
          <p className="rounded-lg border border-destructive/20 bg-destructive/10 p-4 text-sm text-destructive">
            {error}
          </p>
        )}
        <form onSubmit={handleSubmit} className="space-y-5">
          <div className="space-y-2">
            <Label htmlFor="username">Usuario</Label>
            <Input
              id="username"
              type="text"
              value={username}
              onChange={e => setUsername(e.target.value)}
              required
              autoComplete="username"
              placeholder="Usuario"
            />
          </div>
          <div className="space-y-2">
            <Label htmlFor="password">Contraseña</Label>
            <Input
              id="password"
              type="password"
              value={password}
              onChange={e => setPassword(e.target.value)}
              required
              autoComplete="current-password"
              placeholder="Contraseña"
            />
          </div>
          <Button type="submit" className="w-full" disabled={loading}>
            {loading ? 'Entrando…' : 'Entrar'}
          </Button>
        </form>
        <p className="mt-4 text-sm text-muted-foreground leading-relaxed">
          ¿No tienes cuenta? <Link href="/register" className="font-medium text-primary hover:underline">Crear cuenta</Link>
        </p>
        <p className="text-sm text-muted-foreground leading-relaxed">
          Usuario demo: <strong>demo</strong> / <strong>Demo123!</strong>
        </p>
        </CardContent>
      </Card>
    </div>
  );
}

export default function LoginPage() {
  return (
    <Suspense fallback={
      <div className="mx-auto max-w-[420px] px-4 py-10">
        <Card><CardContent className="py-8 text-center text-muted-foreground">Cargando…</CardContent></Card>
      </div>
    }>
      <LoginForm />
    </Suspense>
  );
}
