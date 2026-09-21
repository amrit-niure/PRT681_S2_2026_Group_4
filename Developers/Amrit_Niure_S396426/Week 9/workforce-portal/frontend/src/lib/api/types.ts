// Shapes returned by / sent to the WorkforceApi. Kept in sync by hand with the DTOs in backend/WorkforceApi/Dtos.

export interface Department {
  id: number;
  name: string;
  description: string | null;
  employeeCount: number;
}

export interface DepartmentInput {
  name: string;
  description: string | null;
}

export interface Employee {
  id: number;
  firstName: string;
  lastName: string;
  email: string;
  jobTitle: string;
  departmentId: number;
  departmentName: string;
  /** Plain date, yyyy-MM-dd. */
  hireDate: string;
  salary: number;
  isActive: boolean;
}

export interface EmployeeInput {
  firstName: string;
  lastName: string;
  email: string;
  jobTitle: string;
  departmentId: number;
  hireDate: string;
  salary: number;
  isActive: boolean;
}

export interface PagedResult<T> {
  items: T[];
  total: number;
}

export interface EmployeeQuery {
  search?: string;
  departmentId?: number;
  isActive?: boolean;
  sortBy?: string;
  sortDir?: "asc" | "desc";
  skip?: number;
  take?: number;
}

export interface Shift {
  id: number;
  employeeId: number;
  employeeName: string;
  title: string;
  /** ISO 8601, UTC. */
  start: string;
  /** ISO 8601, UTC. */
  end: string;
  isAllDay: boolean;
  notes: string | null;
}

export interface ShiftInput {
  employeeId: number;
  title: string;
  start: string;
  end: string;
  isAllDay: boolean;
  notes: string | null;
}

export interface ShiftQuery {
  from?: string;
  to?: string;
  employeeId?: number;
}

export interface DashboardSummary {
  totalEmployees: number;
  activeEmployees: number;
  departmentCount: number;
  upcomingShifts: number;
  averageSalary: number;
  headcountByDepartment: { department: string; count: number }[];
  hiresByYear: { year: number; count: number }[];
  shiftHoursByDay: { date: string; hours: number }[];
}
