import { Trash2 } from 'lucide-react'
import type { TodoItem } from '../api/todos'
import { Button } from '@/components/ui/button'
import { Checkbox } from '@/components/ui/checkbox'
import { Label } from '@/components/ui/label'
import { cn } from 'cn'

interface TodoListProps {
  items: TodoItem[]
  onToggle: (item: TodoItem) => void
  onDelete: (id: number) => void
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
      {items.map((item) => (
        <li key={item.id} className="flex items-center justify-between gap-2 py-2">
          <Label className="flex-1 cursor-pointer font-normal">
            <Checkbox
              checked={item.isComplete}
              onCheckedChange={() => onToggle(item)}
            />
            <span
              className={cn(
                item.isComplete && 'text-muted-foreground line-through',
              )}
            >
              {item.title}
            </span>
          </Label>
          <Button
            type="button"
            variant="ghost"
            size="icon-sm"
            className="text-destructive hover:text-destructive"
            onClick={() => onDelete(item.id)}
            aria-label={`Delete ${item.title}`}
          >
            <Trash2 />
          </Button>
        </li>
      ))}
    </ul>
  )
}
