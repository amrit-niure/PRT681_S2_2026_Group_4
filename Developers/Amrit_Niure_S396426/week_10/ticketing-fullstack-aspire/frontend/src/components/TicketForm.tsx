import { useState, type FormEvent } from 'react'
import { Loader2, Plus } from 'lucide-react'
import { Button } from '@/components/ui/button'
import { Input } from '@/components/ui/input'
import { cn } from 'cn'
import { PRIORITIES, type CreateTicket } from '../api/tickets'
import { selectClassName } from './selectClassName'

interface TicketFormProps {
  onCreate: (input: CreateTicket) => Promise<void>
}

/** Form for raising a new ticket. Keeps its own input state and hands the trimmed values up. */
export function TicketForm({ onCreate }: TicketFormProps) {
  const [title, setTitle] = useState('')
  const [description, setDescription] = useState('')
  const [priority, setPriority] = useState(1)
  const [busy, setBusy] = useState(false)

  async function handleSubmit(event: FormEvent) {
    event.preventDefault()
    if (busy || !title.trim() || !description.trim()) return

    setBusy(true)
    try {
      await onCreate({ title: title.trim(), description: description.trim(), priority })
      setTitle('')
      setDescription('')
      setPriority(1)
    } finally {
      setBusy(false)
    }
  }

  return (
    <form className="flex flex-col gap-2" onSubmit={handleSubmit}>
      <Input
        type="text"
        placeholder="Ticket title"
        value={title}
        maxLength={200}
        onChange={(e) => setTitle(e.target.value)}
        aria-label="Ticket title"
      />
      <textarea
        placeholder="Describe the problem"
        value={description}
        maxLength={4000}
        rows={3}
        onChange={(e) => setDescription(e.target.value)}
        aria-label="Ticket description"
        className={cn(
          'w-full min-w-0 rounded-lg border border-input bg-transparent px-2.5 py-1.5 text-base outline-none placeholder:text-muted-foreground focus-visible:border-ring focus-visible:ring-3 focus-visible:ring-ring/50 md:text-sm dark:bg-input/30',
        )}
      />
      <div className="flex flex-wrap gap-2">
        <select
          className={selectClassName}
          value={priority}
          onChange={(e) => setPriority(Number(e.target.value))}
          aria-label="Priority"
        >
          {PRIORITIES.map((label, value) => (
            <option key={label} value={value}>
              {label} priority
            </option>
          ))}
        </select>
        <Button type="submit" disabled={busy || !title.trim() || !description.trim()}>
          {busy ? <Loader2 className="animate-spin" /> : <Plus />}
          Raise ticket
        </Button>
      </div>
    </form>
  )
}
