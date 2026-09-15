import { useState, type FormEvent } from 'react'
import { Loader2, Plus } from 'lucide-react'
import { Button } from '@/components/ui/button'
import { Input } from '@/components/ui/input'
import { cn } from 'cn'

interface AddTodoFormProps {
  onAdd: (
    title: string,
    dueDate: string | null,
    reminderMinutesBefore: number | null,
  ) => Promise<void>
}

const REMINDER_OPTIONS: { value: string; label: string }[] = [
  { value: '', label: 'No reminder' },
  { value: '0', label: 'At due time' },
  { value: '15', label: '15 minutes before' },
  { value: '30', label: '30 minutes before' },
  { value: '60', label: '1 hour before' },
  { value: '1440', label: '1 day before' },
]

const selectClassName = cn(
  'h-8 w-full min-w-0 rounded-lg border border-input bg-transparent px-2.5 py-1 text-base transition-colors outline-none placeholder:text-muted-foreground focus-visible:border-ring focus-visible:ring-3 focus-visible:ring-ring/50 disabled:pointer-events-none disabled:cursor-not-allowed disabled:bg-input/50 disabled:opacity-50 md:text-sm dark:bg-input/30 dark:disabled:bg-input/80',
)

/**
 * Controlled form for adding a task. Keeps its own input state and hands the
 * trimmed title, optional due date/time, and reminder lead time up to the parent on submit.
 */
export function AddTodoForm({ onAdd }: AddTodoFormProps) {
  const [title, setTitle] = useState('')
  const [dueDate, setDueDate] = useState('')
  const [reminderMinutesBefore, setReminderMinutesBefore] = useState('')
  const [busy, setBusy] = useState(false)

  async function handleSubmit(event: FormEvent) {
    event.preventDefault()
    const trimmed = title.trim()
    if (!trimmed || busy) return

    setBusy(true)
    try {
      // dueDate is the browser's local wall-clock time; convert to a UTC instant
      // so the server's "now >= dueDate - lead time" comparison lines up regardless
      // of the signed-in user's timezone.
      const dueDateUtc = dueDate ? new Date(dueDate).toISOString() : null
      const reminder = dueDateUtc && reminderMinutesBefore !== ''
        ? Number(reminderMinutesBefore)
        : null
      await onAdd(trimmed, dueDateUtc, reminder)
      setTitle('')
      setDueDate('')
      setReminderMinutesBefore('')
    } finally {
      setBusy(false)
    }
  }

  return (
    <form className="flex flex-wrap gap-2" onSubmit={handleSubmit}>
      <Input
        type="text"
        placeholder="What needs doing?"
        value={title}
        onChange={(e) => setTitle(e.target.value)}
        aria-label="New task title"
        className="min-w-40 flex-1"
      />
      <Input
        type="datetime-local"
        className="w-48 shrink-0"
        value={dueDate}
        onChange={(e) => setDueDate(e.target.value)}
        aria-label="Due date and time"
      />
      <select
        className={cn(selectClassName, 'w-40 shrink-0')}
        value={reminderMinutesBefore}
        onChange={(e) => setReminderMinutesBefore(e.target.value)}
        disabled={!dueDate}
        aria-label="Remind me"
      >
        {REMINDER_OPTIONS.map((option) => (
          <option key={option.value} value={option.value}>
            {option.label}
          </option>
        ))}
      </select>
      <Button type="submit" disabled={busy || title.trim() === ''}>
        {busy ? <Loader2 className="animate-spin" /> : <Plus />}
        Add
      </Button>
    </form>
  )
}
