// Shared HTTP client: knows the API base URL, stores the auth tokens, attaches the
// bearer header, and transparently refreshes an expired access token once.

export const API_URL = import.meta.env.VITE_API_URL ?? 'http://localhost:5258'

const STORAGE_KEY = 'todo.auth'

export interface Auth {
  accessToken: string
  refreshToken: string
  email: string
}

export class UnauthorizedError extends Error {
  constructor() {
    super('Your session has expired. Please sign in again.')
    this.name = 'UnauthorizedError'
  }
}

let auth: Auth | null = readAuth()
const listeners = new Set<(auth: Auth | null) => void>()

function readAuth(): Auth | null {
  try {
    const raw = localStorage.getItem(STORAGE_KEY)
    return raw ? (JSON.parse(raw) as Auth) : null
  } catch {
    return null
  }
}

export function getAuth(): Auth | null {
  return auth
}

export function setAuth(next: Auth | null): void {
  auth = next
  try {
    if (next) localStorage.setItem(STORAGE_KEY, JSON.stringify(next))
    else localStorage.removeItem(STORAGE_KEY)
  } catch {
    // Ignore storage failures (private mode etc.); state still lives in memory.
  }
  listeners.forEach((fn) => fn(next))
}

/** Subscribe to sign-in / sign-out. Returns an unsubscribe function. */
export function onAuthChange(fn: (auth: Auth | null) => void): () => void {
  listeners.add(fn)
  return () => listeners.delete(fn)
}

async function tryRefresh(): Promise<boolean> {
  if (!auth) return false
  try {
    const res = await fetch(`${API_URL}/api/auth/refresh`, {
      method: 'POST',
      headers: { 'Content-Type': 'application/json' },
      body: JSON.stringify({ refreshToken: auth.refreshToken }),
    })
    if (!res.ok) return false
    const data = (await res.json()) as { accessToken: string; refreshToken: string }
    setAuth({ ...auth, accessToken: data.accessToken, refreshToken: data.refreshToken })
    return true
  } catch {
    return false
  }
}

/** fetch() with the bearer token attached and a one-shot refresh on 401. */
export async function authFetch(path: string, init: RequestInit = {}): Promise<Response> {
  const call = () =>
    fetch(`${API_URL}${path}`, {
      ...init,
      headers: {
        ...init.headers,
        ...(auth ? { Authorization: `Bearer ${auth.accessToken}` } : {}),
      },
    })

  let res = await call()
  if (res.status === 401 && auth) {
    if (await tryRefresh()) {
      res = await call()
    }
    if (res.status === 401) {
      setAuth(null)
      throw new UnauthorizedError()
    }
  }
  return res
}

export async function readJson<T>(res: Response): Promise<T> {
  if (!res.ok) {
    throw new Error(`Request failed: ${res.status} ${res.statusText}`)
  }
  return (res.status === 204 ? undefined : await res.json()) as T
}
