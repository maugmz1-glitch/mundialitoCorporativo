import NextAuth from 'next-auth';
import Credentials from 'next-auth/providers/credentials';

const BACKEND_URL = process.env.API_URL || process.env.NEXT_PUBLIC_API_URL || 'http://127.0.0.1:5000';

const authSecret = process.env.AUTH_SECRET ?? process.env.NEXTAUTH_SECRET;
if (!authSecret) {
  throw new Error(
    '[NextAuth] Falta AUTH_SECRET. En la carpeta frontend crea .env.local con una línea AUTH_SECRET=... (generar: node scripts/generate-auth-secret.js)'
  );
}

export const { handlers, signIn, signOut, auth } = NextAuth({
  secret: authSecret,
  providers: [
    Credentials({
      credentials: {
        username: { label: 'Usuario', type: 'text' },
        password: { label: 'Contraseña', type: 'password' },
      },
      authorize: async (credentials) => {
        const username = credentials?.username as string | undefined;
        const password = credentials?.password as string | undefined;
        if (!username?.trim() || !password) return null;

        const url = `${BACKEND_URL.replace(/\/$/, '')}/api/v1/auth/login`;
        let res: Response;
        try {
          res = await fetch(url, {
            method: 'POST',
            headers: { 'Content-Type': 'application/json' },
            body: JSON.stringify({ username: username.trim(), password }),
          });
        } catch (e) {
          const msg = e instanceof Error ? e.message : '';
          if (msg === 'fetch failed' || msg.includes('ECONNREFUSED') || msg.includes('Failed to fetch')) {
            throw new Error('No se pudo conectar con el servidor. Comprueba que la API esté en ejecución (puerto 5000 o docker compose up).');
          }
          throw e;
        }
        if (!res.ok) {
          const err = await res.json().catch(() => ({}));
          const message = (err as { message?: string }).message || 'Credenciales inválidas';
          throw new Error(message);
        }
        const data = (await res.json()) as { token: string; userName: string };
        return {
          id: data.userName,
          name: data.userName,
          backendToken: data.token,
        };
      },
    }),
  ],
  callbacks: {
    jwt: ({ token, user }) => {
      if (user?.backendToken) {
        token.backendToken = user.backendToken;
        token.name = user.name;
        token.sub = user.id;
      }
      return token;
    },
    session: ({ session, token }) => {
      if (session.user) {
        session.user.name = token.name ?? session.user.name;
        (session as SessionWithToken).backendToken = token.backendToken as string | undefined;
      }
      return session;
    },
  },
  pages: {
    signIn: '/login',
  },
  session: {
    strategy: 'jwt',
    maxAge: 24 * 60 * 60, // 24 horas, alineado con el JWT del backend
  },
  trustHost: true,
});

export type SessionWithToken = { backendToken?: string };
declare module 'next-auth' {
  interface Session {
    backendToken?: string;
  }
  interface User {
    backendToken?: string;
  }
}
