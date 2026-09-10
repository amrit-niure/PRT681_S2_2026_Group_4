import { useEffect, useState } from 'react'
import { BellRing, CircleAlert, Loader2, LogOut } from 'lucide-react'
import {
  addTodo,
  deleteTodo,
  getTodos,
  runReminders,
  updateTodo,
  type TodoItem,
} from './api/todos'
import { AddTodoForm } from './components/AddTodoForm'
import { AuthForm } from './components/AuthForm'
import { TodoList } from './components/TodoList'
import { Button } from '@/components/ui/button'
import { useAuth } from './auth/AuthContext'

function TasksScreen() {
  const { email, signOut } = useAuth()
  const [items, setItems] = useState<TodoItem[]>([])
  const [loading, setLoading] = useState(true)
  const [error, setError] = useState<string | null>(null)
  const [reminding, setReminding] = useState(false)
  const [notice, setNotice] = useState<string | null>(null)

  useEffect(() => {
    getTodos()
      .then(setItems)
      .catch((e: unknown) => setError(e instanceof Error ? e.message : 'Failed to load tasks'))
      .finally(() => setLoading(false))
  }, [])

  async function handleAdd(title: string, dueDate: string | null) {
    const created = await addTodo({ title, isComplete: false, dueDate })
    setItems((prev) => [created, ...prev])
  }

  async function handleToggle(item: TodoItem) {
    const next = {
      title: item.title,
      isComplete: !item.isComplete,
      dueDate: item.dueDate,
    }
    await updateTodo(item.id, next)
    setItems((prev) => prev.map((t) => (t.id === item.id ? { ...t, ...next } : t)))
  }

  async function handleDelete(id: number) {
    await deleteTodo(id)
    setItems((prev) => prev.filter((t) => t.id !== id))
  }

  async function handleRemindNow() {
    setReminding(true)
    setNotice(null)
    try {
      const { remindedCount } = await runReminders()
      setNotice(
        remindedCount === 0
          ? 'No due tasks to email right now.'
          : `Emailed you ${remindedCount} due task${remindedCount === 1 ? '' : 's'}.`,
      )
    } catch (e: unknown) {
      setNotice(e instanceof Error ? e.message : 'Could not send reminders.')
    } finally {
      setReminding(false)
    }
  }

  return (
    <main className="mx-auto flex max-w-lg flex-col gap-4 px-4 py-12">
      <header className="flex items-center justify-between gap-2">
        <h1 className="text-xl font-semibold">Tasks</h1>
        <div className="flex items-center gap-2">
          <Button
            type="button"
            variant="outline"
            size="sm"
            onClick={handleRemindNow}
            disabled={reminding}
          >
            {reminding ? <Loader2 className="animate-spin" /> : <BellRing />}
            Remind me
          </Button>
          <Button type="button" variant="ghost" size="sm" onClick={signOut}>
            <LogOut />
            Sign out
          </Button>
        </div>
      </header>
      <p className="-mt-2 text-sm text-muted-foreground">{email}</p>

      <AddTodoForm onAdd={handleAdd} />

      {notice && <p className="text-sm text-muted-foreground">{notice}</p>}

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
        <TodoList items={items} onToggle={handleToggle} onDelete={handleDelete} />
      )}
    </main>
  )
}

function App() {
  const { isAuthenticated } = useAuth()

  if (!isAuthenticated) {
    return (
      <main className="mx-auto flex max-w-lg flex-col gap-4 px-4 py-12">
        <h1 className="text-xl font-semibold">Tasks</h1>
        <AuthForm />
      </main>
    )
  }

  return <TasksScreen />
}

export default App
