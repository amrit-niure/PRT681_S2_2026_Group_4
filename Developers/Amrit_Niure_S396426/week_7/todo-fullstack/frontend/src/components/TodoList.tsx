import type { TodoItem } from '../api/todos'

interface TodoListProps {
  items: TodoItem[]
  onToggle: (item: TodoItem) => void
  onDelete: (id: number) => void
}

/** Renders the list of tasks with a checkbox to toggle and a button to delete. */
export function TodoList({ items, onToggle, onDelete }: TodoListProps) {
  if (items.length === 0) {
    return <p className="empty">No tasks yet. Add one above.</p>
  }

  return (
    <ul className="todo-list">
      {items.map((item) => (
        <li key={item.id} className={item.isComplete ? 'done' : undefined}>
          <label>
            <input
              type="checkbox"
              checked={item.isComplete}
              onChange={() => onToggle(item)}
            />
            <span>{item.title}</span>
          </label>
          <button
            type="button"
            className="delete"
            onClick={() => onDelete(item.id)}
            aria-label={`Delete ${item.title}`}
          >
            &times;
          </button>
        </li>
      ))}
    </ul>
  )
}
