import { MessageSquare, Trash2, UserRound } from 'lucide-react'
import {
  AlertDialog,
  AlertDialogAction,
  AlertDialogCancel,
  AlertDialogContent,
  AlertDialogDescription,
  AlertDialogFooter,
  AlertDialogHeader,
  AlertDialogTitle,
  AlertDialogTrigger,
} from '@/components/ui/alert-dialog'
import { Button } from '@/components/ui/button'
import { cn } from 'cn'
import { PRIORITIES, STATUSES, formatTimestamp, type Ticket } from '../api/tickets'

interface TicketListProps {
  tickets: Ticket[]
  currentEmail: string | null
  onOpen: (ticket: Ticket) => void
  onDelete: (id: number) => void
}

const PRIORITY_STYLES = [
  'bg-muted text-muted-foreground',
  'bg-blue-500/15 text-blue-700 dark:text-blue-300',
  'bg-amber-500/15 text-amber-700 dark:text-amber-300',
  'bg-red-500/15 text-red-700 dark:text-red-300',
]

const STATUS_STYLES = [
  'bg-emerald-500/15 text-emerald-700 dark:text-emerald-300',
  'bg-violet-500/15 text-violet-700 dark:text-violet-300',
  'bg-muted text-muted-foreground',
  'bg-muted text-muted-foreground',
]

function Badge({ className, children }: { className: string; children: string }) {
  return (
    <span className={cn('rounded-full px-2 py-0.5 text-xs font-medium', className)}>{children}</span>
  )
}

export function TicketList({ tickets, currentEmail, onOpen, onDelete }: TicketListProps) {
  if (tickets.length === 0) {
    return <p className="text-sm text-muted-foreground">No tickets here yet.</p>
  }

  return (
    <ul className="flex flex-col gap-2">
      {tickets.map((ticket) => (
        <li
          key={ticket.id}
          className="flex items-start gap-3 rounded-lg border bg-card p-3 text-card-foreground"
        >
          <button
            type="button"
            className="flex min-w-0 flex-1 flex-col gap-1.5 text-left"
            onClick={() => onOpen(ticket)}
          >
            <span className="flex flex-wrap items-center gap-2">
              <span className="text-xs text-muted-foreground">#{ticket.id}</span>
              <span
                className={cn(
                  'font-medium',
                  ticket.status >= 2 && 'text-muted-foreground line-through',
                )}
              >
                {ticket.title}
              </span>
              <Badge className={STATUS_STYLES[ticket.status]}>{STATUSES[ticket.status]}</Badge>
              <Badge className={PRIORITY_STYLES[ticket.priority]}>{PRIORITIES[ticket.priority]}</Badge>
            </span>
            <span className="flex flex-wrap items-center gap-x-3 gap-y-1 text-xs text-muted-foreground">
              <span>
                {ticket.createdBy} · {formatTimestamp(ticket.createdAt)}
              </span>
              <span className="flex items-center gap-1">
                <UserRound className="size-3" />
                {ticket.assignedTo ?? 'Unassigned'}
              </span>
              <span className="flex items-center gap-1">
                <MessageSquare className="size-3" />
                {ticket.commentCount}
              </span>
            </span>
          </button>

          {ticket.createdBy === currentEmail && (
            <AlertDialog>
              <AlertDialogTrigger
                render={
                  <Button type="button" variant="ghost" size="icon-sm" aria-label={`Delete ticket ${ticket.id}`} />
                }
              >
                <Trash2 />
              </AlertDialogTrigger>
              <AlertDialogContent>
                <AlertDialogHeader>
                  <AlertDialogTitle>Delete ticket #{ticket.id}?</AlertDialogTitle>
                  <AlertDialogDescription>
                    "{ticket.title}" and its comments will be permanently removed.
                  </AlertDialogDescription>
                </AlertDialogHeader>
                <AlertDialogFooter>
                  <AlertDialogCancel>Cancel</AlertDialogCancel>
                  <AlertDialogAction onClick={() => onDelete(ticket.id)}>Delete</AlertDialogAction>
                </AlertDialogFooter>
              </AlertDialogContent>
            </AlertDialog>
          )}
        </li>
      ))}
    </ul>
  )
}
