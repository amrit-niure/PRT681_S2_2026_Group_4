// Talks to the Todo API. Every call carries the signed-in user's bearer token
// via authFetch, so the server scopes the results to that user.

import { authFetch, readJson } from './client'

const ENDPOINT = '/api/todoitems'

export interface TodoItem {
  id: number
  title: string
  isComplete: boolean
  dueDate: string | null
  createdAt: string
}

export interface SaveTodoItem {
  title: string
  isComplete: boolean
  dueDate: string | null
}

export function getTodos(): Promise<TodoItem[]> {
  return authFetch(ENDPOINT).then((r) => readJson<TodoItem[]>(r))
}

export function addTodo(input: SaveTodoItem): Promise<TodoItem> {
  return authFetch(ENDPOINT, {
    method: 'POST',
    headers: { 'Content-Type': 'application/json' },
    body: JSON.stringify(input),
  }).then((r) => readJson<TodoItem>(r))
}

export function updateTodo(id: number, input: SaveTodoItem): Promise<void> {
  return authFetch(`${ENDPOINT}/${id}`, {
    method: 'PUT',
    headers: { 'Content-Type': 'application/json' },
    body: JSON.stringify(input),
  }).then((r) => readJson<void>(r))
}

export function deleteTodo(id: number): Promise<void> {
  return authFetch(`${ENDPOINT}/${id}`, { method: 'DELETE' }).then((r) =>
    readJson<void>(r),
  )
}

/** Ask the server to email the signed-in user their overdue tasks now. */
export function runReminders(): Promise<{ remindedCount: number }> {
  return authFetch('/api/reminders/run', { method: 'POST' }).then((r) =>
    readJson<{ remindedCount: number }>(r),
  )
}
