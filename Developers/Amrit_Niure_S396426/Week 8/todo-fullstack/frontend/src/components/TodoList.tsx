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

/** Formats a due date for display, e.g. "15 Sep 2026". */
function formatDueDate(iso: string): string {
  return new Date(iso).toLocaleDateString(undefined, {
    day: 'numeric',
    month: 'short',
    year: 'numeric',
  })
}

/** A task is overdue when it has a past due date and isn't done yet. */
function isOverdue(item: TodoItem): boolean {
  if (!item.dueDate || item.isComplete) return false
  const due = new Date(item.dueDate)
  due.setHours(0, 0, 0, 0)
  const today = new Date()
  today.setHours(0, 0, 0, 0)
  return due < today
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
                  </span>
                )}
              </span>
            </Label>

            <AlertDialog>
              <AlertDialogTrigger asChild>
                <Button
                  type="button"
                  variant="ghost"
                  size="icon-sm"
                  className="text-destructive hover:text-destructive"
                  aria-label={`Delete ${item.title}`}
                >
                  <Trash2 />
                </Button>
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
