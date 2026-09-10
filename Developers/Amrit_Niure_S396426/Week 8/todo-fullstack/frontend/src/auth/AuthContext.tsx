import {
  createContext,
  useCallback,
  useContext,
  useEffect,
  useMemo,
  useState,
  type ReactNode,
} from 'react'
import { getAuth, onAuthChange } from '../api/client'
import { login as loginApi, logout as logoutApi, register as registerApi } from '../api/auth'

interface AuthContextValue {
  email: string | null
  isAuthenticated: boolean
  signIn: (email: string, password: string) => Promise<void>
  signUp: (email: string, password: string) => Promise<void>
  signOut: () => void
}

const AuthContext = createContext<AuthContextValue | null>(null)

export function AuthProvider({ children }: { children: ReactNode }) {
  const [email, setEmail] = useState<string | null>(() => getAuth()?.email ?? null)

  // Keep context state in sync with the token store (refresh, expiry, other tabs).
  useEffect(() => onAuthChange((auth) => setEmail(auth?.email ?? null)), [])

  const signIn = useCallback(async (e: string, p: string) => {
    await loginApi(e, p)
  }, [])

  const signUp = useCallback(async (e: string, p: string) => {
    await registerApi(e, p)
    await loginApi(e, p)
  }, [])

  const signOut = useCallback(() => logoutApi(), [])

  const value = useMemo<AuthContextValue>(
    () => ({ email, isAuthenticated: email !== null, signIn, signUp, signOut }),
    [email, signIn, signUp, signOut],
  )

  return <AuthContext.Provider value={value}>{children}</AuthContext.Provider>
}

// eslint-disable-next-line react-refresh/only-export-components
export function useAuth(): AuthContextValue {
  const ctx = useContext(AuthContext)
  if (!ctx) throw new Error('useAuth must be used inside <AuthProvider>')
  return ctx
}
