'use client';

import { useState } from 'react';
import { useRouter } from 'next/navigation';
import Link from 'next/link';
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
      // Usar /api/v1/auth/register para que el proxy envíe al backend (NextAuth captura /api/auth/*).
      const res = await fetch('/api/v1/auth/register', {
        method: 'POST',
        headers: { 'Content-Type': 'application/json' },
        credentials: 'include',
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
    <div className="mx-auto max-w-[420px] px-4 py-10">
      <Card>
        <CardHeader className="space-y-1.5">
          <CardTitle className="text-xl">Crear cuenta</CardTitle>
          <CardDescription className="text-base leading-relaxed">
            Regístrate para poder ingresar al sistema.
          </CardDescription>
        </CardHeader>
        <CardContent className="space-y-4">
        {error && (
          <p className="rounded-lg border border-destructive/20 bg-destructive/10 p-4 text-sm text-destructive">
            {error}
          </p>
        )}
        <form onSubmit={handleSubmit} className="space-y-5">
          <div className="space-y-2">
            <Label htmlFor="userName">Nombre de usuario *</Label>
            <Input
              id="userName"
              value={userName}
              onChange={e => setUserName(e.target.value)}
              required
              minLength={3}
              placeholder="Mínimo 3 caracteres"
              autoComplete="username"
            />
          </div>
          <div className="space-y-2">
            <Label htmlFor="email">Correo (opcional)</Label>
            <Input
              id="email"
              type="email"
              value={email}
              onChange={e => setEmail(e.target.value)}
              placeholder="tu@correo.com"
              autoComplete="email"
            />
          </div>
          <div className="space-y-2">
            <Label htmlFor="password">Contraseña *</Label>
            <Input
              id="password"
              type="password"
              value={password}
              onChange={e => setPassword(e.target.value)}
              required
              minLength={6}
              placeholder="Mínimo 6 caracteres"
              autoComplete="new-password"
            />
          </div>
          <div className="space-y-2">
            <Label htmlFor="confirmPassword">Confirmar contraseña *</Label>
            <Input
              id="confirmPassword"
              type="password"
              value={confirmPassword}
              onChange={e => setConfirmPassword(e.target.value)}
              required
              placeholder="Repite la contraseña"
              autoComplete="new-password"
            />
          </div>
          <Button type="submit" className="w-full" disabled={loading}>
            {loading ? 'Creando cuenta…' : 'Crear cuenta'}
          </Button>
        </form>
        <p className="mt-4 text-sm text-muted-foreground leading-relaxed">
          ¿Ya tienes cuenta? <Link href="/login" className="font-medium text-primary hover:underline">Iniciar sesión</Link>
        </p>
        </CardContent>
      </Card>
    </div>
  );
}
