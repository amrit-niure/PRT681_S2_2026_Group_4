import { useState, type FormEvent } from 'react'

interface AddTodoFormProps {
  onAdd: (title: string) => Promise<void>
}

/**
 * Controlled form for adding a task. Keeps its own input state and hands the
 * trimmed title up to the parent on submit.
 */
export function AddTodoForm({ onAdd }: AddTodoFormProps) {
  const [title, setTitle] = useState('')
  const [busy, setBusy] = useState(false)

  async function handleSubmit(event: FormEvent) {
    event.preventDefault()
    const trimmed = title.trim()
    if (!trimmed || busy) return

    setBusy(true)
    try {
      await onAdd(trimmed)
      setTitle('')
    } finally {
      setBusy(false)
    }
  }

  return (
    <form className="add-form" onSubmit={handleSubmit}>
      <input
        type="text"
        placeholder="What needs doing?"
        value={title}
        onChange={(e) => setTitle(e.target.value)}
        aria-label="New task title"
      />
      <button type="submit" disabled={busy || title.trim() === ''}>
        Add
      </button>
    </form>
  )
}
