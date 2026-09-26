import { LogOut } from 'lucide-react'
import { AuthForm } from './components/AuthForm'
import { Button } from '@/components/ui/button'
import { useAuth } from './auth/AuthContext'

function App() {
  const { email, isAuthenticated, signOut } = useAuth()

  if (!isAuthenticated) {
    return (
      <main className="mx-auto flex min-h-screen max-w-lg items-center px-4">
        <AuthForm />
      </main>
    )
  }

  return (
    <main className="mx-auto flex max-w-3xl flex-col gap-4 px-4 py-12">
      <header className="flex items-center justify-between gap-2">
        <h1 className="text-xl font-semibold">Tickets</h1>
        <div className="flex items-center gap-3">
          <span className="text-sm text-muted-foreground">{email}</span>
          <Button type="button" variant="ghost" size="sm" onClick={signOut}>
            <LogOut />
            Sign out
          </Button>
        </div>
      </header>
    </main>
  )
}

export default App
