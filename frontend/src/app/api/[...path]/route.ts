/**
 * Proxy de /api/* al backend. Usa API_URL en runtime (Docker: http://api:8080).
 * El token de sesión (httpOnly cookie) se inyecta aquí en el servidor; el cliente nunca ve el JWT.
 */
import { NextRequest, NextResponse } from 'next/server';
import { auth } from '@/auth';

const BACKEND = process.env.API_URL || process.env.NEXT_PUBLIC_API_URL || 'http://127.0.0.1:5000';

type Params = { path?: string[] } | Promise<{ path?: string[] }>;

async function resolvePath(params: Params): Promise<string[]> {
  const resolved = typeof (params as Promise<unknown>)?.then === 'function' ? await (params as Promise<{ path?: string[] }>) : (params as { path?: string[] });
  const path = resolved?.path;
  if (Array.isArray(path) && path.length > 0) return path;
  return [];
}

export async function GET(request: NextRequest, { params }: { params: Params }) {
  return proxy(request, params, 'GET');
}

export async function POST(request: NextRequest, { params }: { params: Params }) {
  return proxy(request, params, 'POST');
}

export async function PUT(request: NextRequest, { params }: { params: Params }) {
  return proxy(request, params, 'PUT');
}

export async function PATCH(request: NextRequest, { params }: { params: Params }) {
  return proxy(request, params, 'PATCH');
}

export async function DELETE(request: NextRequest, { params }: { params: Params }) {
  return proxy(request, params, 'DELETE');
}

async function proxy(
  request: NextRequest,
  params: Params,
  method: string
): Promise<NextResponse> {
  const path = await resolvePath(params);
  const segment = path.join('/');
  if (!segment) {
    return NextResponse.json({ message: 'Ruta de API no válida.' }, { status: 404 });
  }
  const url = `${BACKEND.replace(/\/$/, '')}/api/${segment}${request.nextUrl.search}`;

  const headers = new Headers();
  request.headers.forEach((value, key) => {
    const lower = key.toLowerCase();
    if (lower === 'host' || lower === 'connection' || lower === 'authorization') return;
    headers.set(key, value);
  });

  const session = await auth();
  if (session?.backendToken) {
    headers.set('Authorization', `Bearer ${session.backendToken}`);
  }

  let body: string | undefined;
  if (method !== 'GET' && method !== 'HEAD') {
    try {
      body = await request.text();
    } catch {
      // no body
    }
  }

  try {
    const controller = new AbortController();
    const timeout = setTimeout(() => controller.abort(), 30000);
    const res = await fetch(url, { method, headers, body, signal: controller.signal });
    clearTimeout(timeout);
    const contentType = res.headers.get('content-type') || 'application/json';
    const data = res.status === 204 ? null : await res.text();
    return new NextResponse(data, {
      status: res.status,
      statusText: res.statusText,
      headers: { 'Content-Type': contentType },
    });
  } catch (err) {
    const message = err instanceof Error ? err.message : 'Backend unreachable';
    const detail = err instanceof Error && err.name === 'AbortError'
      ? 'La API no respondió a tiempo.'
      : 'Comprueba que la API esté en ejecución. Desde la raíz del repo: dotnet run --project src/MundialitoCorporativo.Api';
    return NextResponse.json({ message: `${message}. ${detail}` }, { status: 502 });
  }
}
