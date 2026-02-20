const API = process.env.NEXT_PUBLIC_API_URL || '';
const AUTH_TOKEN_KEY = 'mundialito_token';
const AUTH_USER_KEY = 'mundialito_user';

export function getAuthToken(): string | null {
  if (typeof window === 'undefined') return null;
  return localStorage.getItem(AUTH_TOKEN_KEY);
}
export function getAuthUser(): string | null {
  if (typeof window === 'undefined') return null;
  return localStorage.getItem(AUTH_USER_KEY);
}
export function setAuth(token: string, userName: string): void {
  if (typeof window === 'undefined') return;
  localStorage.setItem(AUTH_TOKEN_KEY, token);
  localStorage.setItem(AUTH_USER_KEY, userName);
}
export function clearAuth(): void {
  if (typeof window === 'undefined') return;
  localStorage.removeItem(AUTH_TOKEN_KEY);
  localStorage.removeItem(AUTH_USER_KEY);
}

/** Rutas a la API. La backend acepta tanto /api/... como /api/v1/... */
function apiPath(path: string): string {
  return path.startsWith('/api') ? path : `/api/${path.replace(/^\//, '')}`;
}

function authHeaders(): Record<string, string> {
  const token = typeof window !== 'undefined' ? localStorage.getItem(AUTH_TOKEN_KEY) : null;
  const h: Record<string, string> = { 'Content-Type': 'application/json' };
  if (token) h['Authorization'] = `Bearer ${token}`;
  return h;
}

export type Paged<T> = { data: T[]; pageNumber: number; pageSize: number; totalRecords: number; totalPages: number };

export async function fetchApi<T>(path: string, options?: RequestInit): Promise<T> {
  const url = `${API}${apiPath(path)}`;
  const res = await fetch(url, {
    ...options,
    headers: { ...authHeaders(), ...options?.headers } as HeadersInit,
  });
  if (!res.ok) {
    const err = await res.json().catch(() => ({ message: res.statusText }));
    throw new Error((err as { message?: string }).message || res.statusText);
  }
  return res.json();
}

export async function fetchPaged<T>(path: string, params?: Record<string, string | number | undefined>): Promise<Paged<T>> {
  const q = new URLSearchParams();
  if (params) {
    Object.entries(params).forEach(([k, v]) => { if (v !== undefined && v !== '') q.set(k, String(v)); });
  }
  const url = q.toString() ? `${path}?${q}` : path;
  return fetchApi<Paged<T>>(url);
}

export async function postApi<T>(path: string, body: unknown, idempotencyKey?: string): Promise<T> {
  const headers: Record<string, string> = {};
  if (idempotencyKey) headers['Idempotency-Key'] = idempotencyKey;
  return fetchApi<T>(path, { method: 'POST', body: JSON.stringify(body), headers });
}

export async function putApi<T>(path: string, body: unknown): Promise<T> {
  return fetchApi<T>(path, { method: 'PUT', body: JSON.stringify(body) });
}

export async function patchApi<T>(path: string, body: unknown): Promise<T> {
  return fetchApi<T>(path, { method: 'PATCH', body: JSON.stringify(body) });
}

export async function deleteApi(path: string): Promise<void> {
  const res = await fetch(`${API}${apiPath(path)}`, { method: 'DELETE', headers: authHeaders() });
  if (!res.ok && res.status !== 204) {
    const err = await res.json().catch(() => ({ message: res.statusText }));
    throw new Error((err as { message?: string }).message || res.statusText);
  }
}
