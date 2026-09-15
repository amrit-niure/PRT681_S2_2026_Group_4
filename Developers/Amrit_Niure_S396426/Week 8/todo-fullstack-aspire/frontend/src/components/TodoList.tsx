import { CalendarClock, Trash2 } from 'lucide-react'
import type { TodoItem } from '../api/todos'
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
import { Checkbox } from '@/components/ui/checkbox'
import { Label } from '@/components/ui/label'
import { cn } from 'cn'

interface TodoListProps {
  items: TodoItem[]
  onToggle: (item: TodoItem) => void
  onDelete: (id: number) => void
}

/**
 * The API sends DueDate as a naive UTC timestamp (no "Z"/offset, since it's stored
 * as "timestamp without time zone"). Append "Z" so the browser parses it as UTC
 * instead of misreading it as local time.
 */
function parseUtc(iso: string): Date {
  return new Date(/[Zz]|[+-]\d\d:\d\d$/.test(iso) ? iso : `${iso}Z`)
}

/** Formats a due date for display, e.g. "15 Sep 2026, 2:30 pm". */
function formatDueDate(iso: string): string {
  return parseUtc(iso).toLocaleString(undefined, {
    day: 'numeric',
    month: 'short',
    year: 'numeric',
    hour: 'numeric',
    minute: '2-digit',
  })
}

/** Describes when the reminder fires relative to the due date, e.g. "30 min before". */
function formatReminder(minutesBefore: number): string {
  if (minutesBefore === 0) return 'at due time'
  if (minutesBefore % 1440 === 0) return `${minutesBefore / 1440}d before`
  if (minutesBefore % 60 === 0) return `${minutesBefore / 60}h before`
  return `${minutesBefore}min before`
}

/** A task is overdue when its due date/time has passed and isn't done yet. */
function isOverdue(item: TodoItem): boolean {
  if (!item.dueDate || item.isComplete) return false
  return parseUtc(item.dueDate) < new Date()
}

/** Renders the list of tasks with a checkbox to toggle and a button to delete. */
export function TodoList({ items, onToggle, onDelete }: TodoListProps) {
  if (items.length === 0) {
    return (
      <p className="text-sm text-muted-foreground">No tasks yet. Add one above.</p>
    )
  }

  return (
    <ul className="divide-y">
      {items.map((item) => {
        const overdue = isOverdue(item)

        return (
          <li key={item.id} className="flex items-center justify-between gap-2 py-2">
            <Label className="flex-1 cursor-pointer items-start font-normal">
              <Checkbox
                checked={item.isComplete}
                onCheckedChange={() => onToggle(item)}
                className="mt-0.5"
              />
              <span className="flex flex-col gap-0.5">
                <span
                  className={cn(
                    item.isComplete && 'text-muted-foreground line-through',
                  )}
                >
                  {item.title}
                </span>
                {item.dueDate && (
                  <span
                    className={cn(
                      'flex items-center gap-1 text-xs',
                      overdue ? 'text-destructive' : 'text-muted-foreground',
                    )}
                  >
                    <CalendarClock className="size-3" />
                    Due {formatDueDate(item.dueDate)}
                    {overdue && ' · overdue'}
                    {item.reminderMinutesBefore !== null &&
                      ` · reminder ${formatReminder(item.reminderMinutesBefore)}`}
                  </span>
                )}
              </span>
            </Label>

            <AlertDialog>
              <AlertDialogTrigger
                render={
                  <Button
                    type="button"
                    variant="ghost"
                    size="icon-sm"
                    className="text-destructive hover:text-destructive"
                    aria-label={`Delete ${item.title}`}
                  />
                }
              >
                <Trash2 />
              </AlertDialogTrigger>
              <AlertDialogContent>
                <AlertDialogHeader>
                  <AlertDialogTitle>Delete this task?</AlertDialogTitle>
                  <AlertDialogDescription>
                    “{item.title}” will be permanently removed. This can’t be
                    undone.
                  </AlertDialogDescription>
                </AlertDialogHeader>
                <AlertDialogFooter>
                  <AlertDialogCancel>Cancel</AlertDialogCancel>
                  <AlertDialogAction
                    variant="destructive"
                    onClick={() => onDelete(item.id)}
                  >
                    Delete
                  </AlertDialogAction>
                </AlertDialogFooter>
              </AlertDialogContent>
            </AlertDialog>
          </li>
        )
      })}
    </ul>
  )
}
