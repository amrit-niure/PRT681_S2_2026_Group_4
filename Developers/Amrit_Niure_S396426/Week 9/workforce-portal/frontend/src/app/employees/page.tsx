import type { Metadata } from "next";
import { EmployeesGrid } from "@/components/employees/EmployeesGrid";
import { EMPLOYEE_DEFAULT_SORT, EMPLOYEE_PAGE_SIZE } from "@/components/employees/constants";
import { serverApi } from "@/lib/api/server";

export const metadata: Metadata = { title: "Employees" };

// Rendered on every request: the data changes, so it must never be frozen at build time.
export const dynamic = "force-dynamic";

/**
 * Server Component: it fetches the first page (and the department list for the filter) on the server,
 * so the HTML that reaches the browser already contains the table rows. The client grid then hydrates
 * on top of that markup and handles paging, sorting and editing from there.
 */
export default async function EmployeesPage() {
  const [initialData, departments] = await Promise.all([
    serverApi.employees.list({
      sortBy: EMPLOYEE_DEFAULT_SORT.field,
      sortDir: EMPLOYEE_DEFAULT_SORT.dir,
      skip: 0,
      take: EMPLOYEE_PAGE_SIZE,
    }),
    serverApi.departments.list(),
  ]);

  return (
    <>
      <div className="page-header">
        <div>
          <h1>Employees</h1>
          <p className="page-subtitle">{initialData.total} people across {departments.length} departments</p>
        </div>
      </div>
      <EmployeesGrid initialData={initialData} departments={departments} />
    </>
  );
}
