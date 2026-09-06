// Single place that knows how to talk to the Todo API.
// The base URL comes from an env var so it can change per environment.

const API_URL = import.meta.env.VITE_API_URL ?? 'http://localhost:5258'
const ENDPOINT = `${API_URL}/api/todoitems`

export interface TodoItem {
  id: number
  title: string
  isComplete: boolean
  createdAt: string
}

export interface SaveTodoItem {
  title: string
  isComplete: boolean
}

async function handle<T>(res: Response): Promise<T> {
  if (!res.ok) {
    throw new Error(`Request failed: ${res.status} ${res.statusText}`)
  }
  // 204 No Content has an empty body.
  return (res.status === 204 ? undefined : await res.json()) as T
}

export function getTodos(): Promise<TodoItem[]> {
  return fetch(ENDPOINT).then((r) => handle<TodoItem[]>(r))
}

export function addTodo(input: SaveTodoItem): Promise<TodoItem> {
  return fetch(ENDPOINT, {
    method: 'POST',
    headers: { 'Content-Type': 'application/json' },
    body: JSON.stringify(input),
  }).then((r) => handle<TodoItem>(r))
}

export function updateTodo(id: number, input: SaveTodoItem): Promise<void> {
  return fetch(`${ENDPOINT}/${id}`, {
    method: 'PUT',
    headers: { 'Content-Type': 'application/json' },
    body: JSON.stringify(input),
  }).then((r) => handle<void>(r))
}

export function deleteTodo(id: number): Promise<void> {
  return fetch(`${ENDPOINT}/${id}`, { method: 'DELETE' }).then((r) =>
    handle<void>(r),
  )
}
