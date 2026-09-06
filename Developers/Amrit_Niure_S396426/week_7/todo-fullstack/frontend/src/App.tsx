import { useEffect, useState } from 'react'
import { CircleAlert, Loader2 } from 'lucide-react'
import {
  addTodo,
  deleteTodo,
  getTodos,
  updateTodo,
  type TodoItem,
} from './api/todos'
import { AddTodoForm } from './components/AddTodoForm'
import { TodoList } from './components/TodoList'
import {
  Card,
  CardContent,
  CardHeader,
  CardTitle,
} from '@/components/ui/card'

function App() {
  const [items, setItems] = useState<TodoItem[]>([])
  const [loading, setLoading] = useState(true)
  const [error, setError] = useState<string | null>(null)

  // Load the list once when the app mounts.
  useEffect(() => {
    getTodos()
      .then(setItems)
      .catch((e: unknown) => setError(e instanceof Error ? e.message : 'Failed to load tasks'))
      .finally(() => setLoading(false))
  }, [])

  async function handleAdd(title: string) {
    const created = await addTodo({ title, isComplete: false })
    setItems((prev) => [created, ...prev])
  }

  async function handleToggle(item: TodoItem) {
    const next = { title: item.title, isComplete: !item.isComplete }
    await updateTodo(item.id, next)
    setItems((prev) =>
      prev.map((t) => (t.id === item.id ? { ...t, ...next } : t)),
    )
  }

  async function handleDelete(id: number) {
    await deleteTodo(id)
    setItems((prev) => prev.filter((t) => t.id !== id))
  }

  return (
    <main className="mx-auto max-w-lg px-4 py-12">
      <Card>
        <CardHeader>
          <CardTitle className="text-xl">Tasks</CardTitle>
        </CardHeader>
        <CardContent className="flex flex-col gap-4">
          <AddTodoForm onAdd={handleAdd} />

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
            <TodoList
              items={items}
              onToggle={handleToggle}
              onDelete={handleDelete}
            />
          )}
        </CardContent>
      </Card>
    </main>
  )
}

export default App
