import { authFetch, readJson } from './client'

const ENDPOINT = '/api/tickets'

export const STATUSES = ['Open', 'In progress', 'Resolved', 'Closed'] as const
export const PRIORITIES = ['Low', 'Medium', 'High', 'Urgent'] as const

export interface Ticket {
  id: number
  title: string
  description: string
  status: number
  priority: number
  createdBy: string
  assignedToUserId: string | null
  assignedTo: string | null
  createdAt: string
  updatedAt: string
  commentCount: number
}

export interface TicketComment {
  id: number
  author: string
  body: string
  createdAt: string
}

export interface TicketDetail {
  ticket: Ticket
  comments: TicketComment[]
}

export interface Assignee {
  id: string
  email: string
}

export interface CreateTicket {
  title: string
  description: string
  priority: number
}

export interface UpdateTicket extends CreateTicket {
  status: number
  assignedToUserId: string | null
}

const json = { 'Content-Type': 'application/json' }

export function getTickets(filter: { status?: number; mine?: boolean } = {}): Promise<Ticket[]> {
  const params = new URLSearchParams()
  if (filter.status !== undefined) params.set('status', String(filter.status))
  if (filter.mine) params.set('mine', 'true')
  const query = params.size ? `?${params}` : ''
  return authFetch(`${ENDPOINT}${query}`).then((r) => readJson<Ticket[]>(r))
}

export function getTicket(id: number): Promise<TicketDetail> {
  return authFetch(`${ENDPOINT}/${id}`).then((r) => readJson<TicketDetail>(r))
}

export function createTicket(input: CreateTicket): Promise<Ticket> {
  return authFetch(ENDPOINT, { method: 'POST', headers: json, body: JSON.stringify(input) }).then(
    (r) => readJson<Ticket>(r),
  )
}

export function updateTicket(id: number, input: UpdateTicket): Promise<void> {
  return authFetch(`${ENDPOINT}/${id}`, {
    method: 'PUT',
    headers: json,
    body: JSON.stringify(input),
  }).then((r) => readJson<void>(r))
}

export function deleteTicket(id: number): Promise<void> {
  return authFetch(`${ENDPOINT}/${id}`, { method: 'DELETE' }).then((r) => readJson<void>(r))
}

export function addComment(id: number, body: string): Promise<TicketComment> {
  return authFetch(`${ENDPOINT}/${id}/comments`, {
    method: 'POST',
    headers: json,
    body: JSON.stringify({ body }),
  }).then((r) => readJson<TicketComment>(r))
}

export function getAssignees(): Promise<Assignee[]> {
  return authFetch(`${ENDPOINT}/assignees`).then((r) => readJson<Assignee[]>(r))
}

export function formatTimestamp(iso: string): string {
  const utc = /[Zz]|[+-]\d\d:\d\d$/.test(iso) ? iso : `${iso}Z`
  return new Date(utc).toLocaleString(undefined, {
    day: 'numeric',
    month: 'short',
    year: 'numeric',
    hour: 'numeric',
    minute: '2-digit',
  })
}
