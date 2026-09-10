import { useState, type FormEvent } from 'react'
import { Loader2, Plus } from 'lucide-react'
import { Button } from '@/components/ui/button'
import { Input } from '@/components/ui/input'

interface AddTodoFormProps {
  onAdd: (title: string, dueDate: string | null) => Promise<void>
}

/**
 * Controlled form for adding a task. Keeps its own input state and hands the
 * trimmed title and optional due date up to the parent on submit.
 */
export function AddTodoForm({ onAdd }: AddTodoFormProps) {
  const [title, setTitle] = useState('')
  const [dueDate, setDueDate] = useState('')
  const [busy, setBusy] = useState(false)

  async function handleSubmit(event: FormEvent) {
    event.preventDefault()
    const trimmed = title.trim()
    if (!trimmed || busy) return

    setBusy(true)
    try {
      await onAdd(trimmed, dueDate || null)
      setTitle('')
      setDueDate('')
    } finally {
      setBusy(false)
    }
  }

  return (
    <form className="flex gap-2" onSubmit={handleSubmit}>
      <Input
        type="text"
        placeholder="What needs doing?"
        value={title}
        onChange={(e) => setTitle(e.target.value)}
        aria-label="New task title"
      />
      <Input
        type="date"
        className="w-36 shrink-0"
        value={dueDate}
        onChange={(e) => setDueDate(e.target.value)}
        aria-label="Due date"
      />
      <Button type="submit" disabled={busy || title.trim() === ''}>
        {busy ? <Loader2 className="animate-spin" /> : <Plus />}
        Add
      </Button>
    </form>
  )
}
