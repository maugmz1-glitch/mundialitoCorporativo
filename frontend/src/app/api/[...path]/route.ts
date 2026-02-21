/**
 * Proxy de /api/* al backend. Usa API_URL en runtime (Docker: http://api:8080).
 * Evita depender de rewrites en next.config.js que se evalúan en build time.
 */
import { NextRequest, NextResponse } from 'next/server';

const BACKEND = process.env.API_URL || process.env.NEXT_PUBLIC_API_URL || 'http://127.0.0.1:5000';

type Params = { path: string[] };

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
  { path }: { path: string[] },
  method: string
): Promise<NextResponse> {
  const segment = path?.length ? path.join('/') : '';
  const url = `${BACKEND.replace(/\/$/, '')}/api/${segment}${request.nextUrl.search}`;

  const headers = new Headers();
  request.headers.forEach((value, key) => {
    const lower = key.toLowerCase();
    if (lower === 'host' || lower === 'connection') return;
    headers.set(key, value);
  });

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
