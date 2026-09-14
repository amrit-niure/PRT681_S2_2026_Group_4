// Auth API calls against the ASP.NET Core Identity endpoints (mapped at /api/auth).

import { API_URL, setAuth } from './client'

interface LoginResponse {
  accessToken: string
  refreshToken: string
}

/** Pulls a readable message out of an Identity error response. */
async function errorMessage(res: Response, fallback: string): Promise<string> {
  try {
    const body = await res.json()
    if (body?.detail) return body.detail as string
    if (body?.errors && typeof body.errors === 'object') {
      const first = Object.values(body.errors as Record<string, string[]>)[0]
      if (Array.isArray(first) && first[0]) return first[0]
    }
  } catch {
    // fall through
  }
  return fallback
}

export async function register(email: string, password: string): Promise<void> {
  const res = await fetch(`${API_URL}/api/auth/register`, {
    method: 'POST',
    headers: { 'Content-Type': 'application/json' },
    body: JSON.stringify({ email, password }),
  })
  if (!res.ok) {
    throw new Error(await errorMessage(res, 'Could not create the account.'))
  }
}

export async function login(email: string, password: string): Promise<void> {
  const res = await fetch(`${API_URL}/api/auth/login`, {
    method: 'POST',
    headers: { 'Content-Type': 'application/json' },
    body: JSON.stringify({ email, password }),
  })
  if (!res.ok) {
    throw new Error(await errorMessage(res, 'Wrong email or password.'))
  }
  const data = (await res.json()) as LoginResponse
  setAuth({ accessToken: data.accessToken, refreshToken: data.refreshToken, email })
}

export function logout(): void {
  setAuth(null)
}
