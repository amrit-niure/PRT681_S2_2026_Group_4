import { useEffect, useState, type FormEvent } from 'react'
import { CircleAlert, Loader2, Send } from 'lucide-react'
import { Button } from '@/components/ui/button'
import {
  Dialog,
  DialogContent,
  DialogDescription,
  DialogHeader,
  DialogTitle,
} from '@/components/ui/dialog'
import { Input } from '@/components/ui/input'
import {
  PRIORITIES,
  STATUSES,
  addComment,
  formatTimestamp,
  getAssignees,
  getTicket,
  updateTicket,
  type Assignee,
  type TicketDetail as Detail,
} from '../api/tickets'
import { selectClassName } from './selectClassName'

interface TicketDetailProps {
  ticketId: number | null
  onClose: () => void

  onChanged: () => void
}

export function TicketDetail({ ticketId, onClose, onChanged }: TicketDetailProps) {
  const [detail, setDetail] = useState<Detail | null>(null)
  const [assignees, setAssignees] = useState<Assignee[]>([])
  const [comment, setComment] = useState('')
  const [busy, setBusy] = useState(false)
  const [error, setError] = useState<string | null>(null)

  useEffect(() => {
    if (ticketId === null) return
    let cancelled = false
    Promise.all([getTicket(ticketId), getAssignees()])
      .then(([d, a]) => {
        if (cancelled) return
        setDetail(d)
        setAssignees(a)
      })
      .catch((e: unknown) => {
        if (!cancelled) setError(e instanceof Error ? e.message : 'Failed to load the ticket')
      })
    return () => {
      cancelled = true
    }
  }, [ticketId])

  async function change(patch: { status?: number; assignedToUserId?: string | null }) {
    if (!detail) return
    const { ticket } = detail
    setError(null)
    try {
      await updateTicket(ticket.id, {
        title: ticket.title,
        description: ticket.description,
        priority: ticket.priority,
        status: patch.status ?? ticket.status,
        assignedToUserId:
          patch.assignedToUserId !== undefined ? patch.assignedToUserId : ticket.assignedToUserId,
      })
      setDetail(await getTicket(ticket.id))
      onChanged()
    } catch (e: unknown) {
      setError(e instanceof Error ? e.message : 'Could not update the ticket')
    }
  }

  async function handleComment(event: FormEvent) {
    event.preventDefault()
    if (!detail || busy || !comment.trim()) return
    setBusy(true)
    setError(null)
    try {
      await addComment(detail.ticket.id, comment.trim())
      setComment('')
      setDetail(await getTicket(detail.ticket.id))
      onChanged()
    } catch (e: unknown) {
      setError(e instanceof Error ? e.message : 'Could not add the comment')
    } finally {
      setBusy(false)
    }
  }

  const ticket = detail?.ticket

  return (
    <Dialog open={ticketId !== null} onOpenChange={(open) => !open && onClose()}>
      <DialogContent className="max-h-[85vh] overflow-y-auto sm:max-w-xl">
        <DialogHeader>
          <DialogTitle>{ticket ? `#${ticket.id} · ${ticket.title}` : 'Ticket'}</DialogTitle>
          <DialogDescription>
            {ticket
              ? `Raised by ${ticket.createdBy} on ${formatTimestamp(ticket.createdAt)} · ${PRIORITIES[ticket.priority]} priority`
              : 'Loading…'}
          </DialogDescription>
        </DialogHeader>

        {!ticket && !error && <Loader2 className="size-4 animate-spin text-muted-foreground" />}

        {ticket && (
          <div className="flex flex-col gap-4">
            <p className="text-sm whitespace-pre-wrap">{ticket.description}</p>

            <div className="flex flex-wrap gap-2">
              <select
                className={selectClassName}
                value={ticket.status}
                onChange={(e) => change({ status: Number(e.target.value) })}
                aria-label="Status"
              >
                {STATUSES.map((label, value) => (
                  <option key={label} value={value}>
                    {label}
                  </option>
                ))}
              </select>
              <select
                className={selectClassName}
                value={ticket.assignedToUserId ?? ''}
                onChange={(e) => change({ assignedToUserId: e.target.value || null })}
                aria-label="Assignee"
              >
                <option value="">Unassigned</option>
                {assignees.map((a) => (
                  <option key={a.id} value={a.id}>
                    {a.email}
                  </option>
                ))}
              </select>
            </div>

            <section className="flex flex-col gap-2">
              <h3 className="text-sm font-medium">Comments ({detail.comments.length})</h3>
              {detail.comments.length === 0 && (
                <p className="text-sm text-muted-foreground">No comments yet.</p>
              )}
              {detail.comments.map((c) => (
                <div key={c.id} className="rounded-lg bg-muted/50 p-2.5 text-sm">
                  <div className="text-xs text-muted-foreground">
                    {c.author} · {formatTimestamp(c.createdAt)}
                  </div>
                  <p className="whitespace-pre-wrap">{c.body}</p>
                </div>
              ))}
              <form className="flex gap-2" onSubmit={handleComment}>
                <Input
                  value={comment}
                  maxLength={4000}
                  placeholder="Add a comment"
                  aria-label="New comment"
                  onChange={(e) => setComment(e.target.value)}
                />
                <Button type="submit" disabled={busy || !comment.trim()}>
                  {busy ? <Loader2 className="animate-spin" /> : <Send />}
                  Send
                </Button>
              </form>
            </section>
          </div>
        )}

        {error && (
          <p className="flex items-center gap-2 text-sm text-destructive">
            <CircleAlert className="size-4 shrink-0" />
            {error}
          </p>
        )}
      </DialogContent>
    </Dialog>
  )
}
