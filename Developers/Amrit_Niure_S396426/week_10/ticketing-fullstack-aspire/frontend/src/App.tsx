import { useCallback, useEffect, useState } from 'react'
import { CircleAlert, Loader2, LogOut } from 'lucide-react'
import {
  STATUSES,
  createTicket,
  deleteTicket,
  getTicketStats,
  getTickets,
  type CreateTicket,
  type Ticket,
  type TicketStats as Stats,
} from './api/tickets'
import { AuthForm } from './components/AuthForm'
import { TicketDetail } from './components/TicketDetail'
import { TicketForm } from './components/TicketForm'
import { TicketList } from './components/TicketList'
import { TicketStats } from './components/TicketStats'
import { selectClassName } from './components/selectClassName'
import { Button } from '@/components/ui/button'
import { useAuth } from './auth/AuthContext'

function TicketsScreen() {
  const { email, signOut } = useAuth()
  const [tickets, setTickets] = useState<Ticket[]>([])
  const [stats, setStats] = useState<Stats | null>(null)
  const [loading, setLoading] = useState(true)
  const [error, setError] = useState<string | null>(null)
  const [mine, setMine] = useState(false)
  const [status, setStatus] = useState<number | undefined>(undefined)
  const [openId, setOpenId] = useState<number | null>(null)

  const load = useCallback(() => {
    return Promise.all([getTickets({ status, mine }), getTicketStats()])
      .then(([items, summary]) => {
        setTickets(items)
        setStats(summary)
        setError(null)
      })
      .catch((e: unknown) => setError(e instanceof Error ? e.message : 'Failed to load tickets'))
      .finally(() => setLoading(false))
  }, [status, mine])

  useEffect(() => {
    load()
  }, [load])

  async function handleCreate(input: CreateTicket) {
    await createTicket(input)
    await load()
  }

  async function handleDelete(id: number) {
    try {
      await deleteTicket(id)
      await load()
    } catch (e: unknown) {
      setError(e instanceof Error ? e.message : 'Could not delete the ticket')
    }
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

      {stats && <TicketStats stats={stats} onSelectStatus={setStatus} />}

      <TicketForm onCreate={handleCreate} />

      <div className="flex flex-wrap items-center gap-2">
        <select
          className={selectClassName}
          value={status ?? ''}
          onChange={(e) => setStatus(e.target.value === '' ? undefined : Number(e.target.value))}
          aria-label="Filter by status"
        >
          <option value="">All statuses</option>
          {STATUSES.map((label, value) => (
            <option key={label} value={value}>
              {label}
            </option>
          ))}
        </select>
        <label className="flex items-center gap-2 text-sm">
          <input type="checkbox" checked={mine} onChange={(e) => setMine(e.target.checked)} />
          Only tickets I raised
        </label>
      </div>

      {loading && (
        <p className="flex items-center gap-2 text-sm text-muted-foreground">
          <Loader2 className="size-4 animate-spin" />
          Loading…
        </p>
      )}
      {error && (
        <p className="flex items-center gap-2 text-sm text-destructive">
          <CircleAlert className="size-4" />
          {error}
        </p>
      )}
      {!loading && !error && (
        <TicketList
          tickets={tickets}
          currentEmail={email}
          onOpen={(t) => setOpenId(t.id)}
          onDelete={handleDelete}
        />
      )}

      <TicketDetail key={openId} ticketId={openId} onClose={() => setOpenId(null)} onChanged={load} />
    </main>
  )
}

function App() {
  const { isAuthenticated } = useAuth()

  if (!isAuthenticated) {
    return (
      <main className="mx-auto flex min-h-screen max-w-lg items-center px-4">
        <AuthForm />
      </main>
    )
  }

  return <TicketsScreen />
}

export default App
