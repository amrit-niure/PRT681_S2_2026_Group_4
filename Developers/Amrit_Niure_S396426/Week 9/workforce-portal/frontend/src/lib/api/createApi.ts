import { request, toQueryString } from "./http";
import type {
  DashboardSummary,
  Department,
  DepartmentInput,
  Employee,
  EmployeeInput,
  EmployeeQuery,
  PagedResult,
  Shift,
  ShiftInput,
  ShiftQuery,
} from "./types";

/**
 * Typed client for the WorkforceApi. `baseUrl` decides where it runs: the server uses the API's
 * absolute URL (server-side rendering), the browser uses the same-origin /backend proxy.
 */
export function createApi(baseUrl: string, init?: RequestInit) {
  const get = <T>(path: string) => request<T>(`${baseUrl}${path}`, init);
  const send = <T>(method: string, path: string, body?: unknown) =>
    request<T>(`${baseUrl}${path}`, { ...init, method, body: body === undefined ? undefined : JSON.stringify(body) });

  return {
    departments: {
      list: () => get<Department[]>("/departments"),
      create: (input: DepartmentInput) => send<Department>("POST", "/departments", input),
      update: (id: number, input: DepartmentInput) => send<void>("PUT", `/departments/${id}`, input),
      remove: (id: number) => send<void>("DELETE", `/departments/${id}`),
    },
    employees: {
      list: (query: EmployeeQuery = {}) => get<PagedResult<Employee>>(`/employees${toQueryString(query)}`),
      create: (input: EmployeeInput) => send<Employee>("POST", "/employees", input),
      update: (id: number, input: EmployeeInput) => send<void>("PUT", `/employees/${id}`, input),
      remove: (id: number) => send<void>("DELETE", `/employees/${id}`),
    },
    shifts: {
      list: (query: ShiftQuery = {}) => get<Shift[]>(`/shifts${toQueryString(query)}`),
      create: (input: ShiftInput) => send<Shift>("POST", "/shifts", input),
      update: (id: number, input: ShiftInput) => send<void>("PUT", `/shifts/${id}`, input),
      remove: (id: number) => send<void>("DELETE", `/shifts/${id}`),
    },
    dashboard: {
      summary: (utcOffsetMinutes = 0) =>
        get<DashboardSummary>(`/dashboard/summary${toQueryString({ utcOffsetMinutes })}`),
    },
  };
}

export type Api = ReturnType<typeof createApi>;
