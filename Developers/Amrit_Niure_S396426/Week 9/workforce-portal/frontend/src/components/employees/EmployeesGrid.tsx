"use client";

import { createContext, useContext, useEffect, useMemo, useState } from "react";
import { useRouter } from "next/navigation";
import { Grid, GridColumn, type GridCustomCellProps, type GridPageChangeEvent, type GridSortChangeEvent } from "@progress/kendo-react-grid";
import { Button } from "@progress/kendo-react-buttons";
import { Input } from "@progress/kendo-react-inputs";
import { DropDownList } from "@progress/kendo-react-dropdowns";
import { Loader } from "@progress/kendo-react-indicators";
import { pencilIcon, plusIcon, trashIcon } from "@progress/kendo-svg-icons";
import type { SortDescriptor } from "@progress/kendo-data-query";
import { clientApi } from "@/lib/api/client";
import { ApiError } from "@/lib/api/http";
import type { Department, Employee, EmployeeQuery, PagedResult } from "@/lib/api/types";
import { formatCurrency, formatDate } from "@/lib/format";
import { useDebouncedValue } from "@/lib/useDebouncedValue";
import { useMediaQuery } from "@/lib/useMediaQuery";
import { ConfirmDialog } from "@/components/ConfirmDialog";
import { useToast } from "@/components/Toaster";
import { EmployeeFormDialog } from "./EmployeeFormDialog";
import { EMPLOYEE_DEFAULT_SORT, EMPLOYEE_PAGE_SIZE } from "./constants";

type StatusFilter = "all" | "active" | "inactive";

const statusOptions: { value: StatusFilter; label: string }[] = [
  { value: "active", label: "Active" },
  { value: "inactive", label: "Inactive" },
];

const NameCell = ({ tdProps, dataItem }: GridCustomCellProps) => (
  <td {...tdProps}>
    {dataItem.firstName} {dataItem.lastName}
  </td>
);

const HireDateCell = ({ tdProps, dataItem }: GridCustomCellProps) => <td {...tdProps}>{formatDate(dataItem.hireDate)}</td>;

const SalaryCell = ({ tdProps, dataItem }: GridCustomCellProps) => <td {...tdProps}>{formatCurrency(dataItem.salary)}</td>;

const StatusCell = ({ tdProps, dataItem }: GridCustomCellProps) => (
  <td {...tdProps}>
    <span className={`status-pill ${dataItem.isActive ? "status-pill--active" : "status-pill--inactive"}`}>
      {dataItem.isActive ? "Active" : "Inactive"}
    </span>
  </td>
);

// Grid cells are rendered by Kendo, so they can't take extra props; the row buttons reach the grid's
// handlers through context instead.
const RowActionsContext = createContext<{ onEdit: (employee: Employee) => void; onDelete: (employee: Employee) => void }>({
  onEdit: () => {},
  onDelete: () => {},
});

const ActionsCell = ({ tdProps, dataItem }: GridCustomCellProps) => {
  const { onEdit, onDelete } = useContext(RowActionsContext);
  const name = `${dataItem.firstName} ${dataItem.lastName}`;
  return (
    <td {...tdProps}>
      <Button svgIcon={pencilIcon} fillMode="flat" size="small" title={`Edit ${name}`} aria-label={`Edit ${name}`} onClick={() => onEdit(dataItem)} />
      <Button
        svgIcon={trashIcon}
        fillMode="flat"
        size="small"
        themeColor="error"
        title={`Delete ${name}`}
        aria-label={`Delete ${name}`}
        onClick={() => onDelete(dataItem)}
      />
    </td>
  );
};

interface EmployeesGridProps {
  /** First page, fetched on the server during SSR; shown immediately and reused until the query changes. */
  initialData: PagedResult<Employee>;
  departments: Department[];
}

/**
 * Employee table with server-side paging, sorting and filtering: the Grid only ever holds the current
 * page, and every change of page, sort or filter asks the API for exactly the rows it needs.
 */
export function EmployeesGrid({ initialData, departments }: EmployeesGridProps) {
  const { notify } = useToast();
  const router = useRouter();

  const [editing, setEditing] = useState<Employee | "new" | null>(null);
  const [deleting, setDeleting] = useState<Employee | null>(null);
  const [deleteBusy, setDeleteBusy] = useState(false);
  // Bumped after a save/delete so the grid re-queries even though page, sort and filters haven't changed.
  const [refreshCount, setRefreshCount] = useState(0);

  const [searchText, setSearchText] = useState("");
  const [departmentId, setDepartmentId] = useState<number | null>(null);
  const [status, setStatus] = useState<StatusFilter>("all");
  const [sort, setSort] = useState<SortDescriptor[]>([EMPLOYEE_DEFAULT_SORT]);
  const [paging, setPaging] = useState({ skip: 0, take: EMPLOYEE_PAGE_SIZE });
  const search = useDebouncedValue(searchText.trim(), 300);

  const apiQuery = useMemo<EmployeeQuery>(
    () => ({
      search: search || undefined,
      departmentId: departmentId ?? undefined,
      isActive: status === "all" ? undefined : status === "active",
      sortBy: sort[0]?.field,
      sortDir: sort[0]?.dir,
      skip: paging.skip,
      take: paging.take,
    }),
    [search, departmentId, status, sort, paging],
  );

  // What's on screen is tagged with the query that produced it; "loading" is simply "the query has moved on".
  const initialKey = JSON.stringify({
    sortBy: EMPLOYEE_DEFAULT_SORT.field,
    sortDir: EMPLOYEE_DEFAULT_SORT.dir,
    skip: 0,
    take: EMPLOYEE_PAGE_SIZE,
    refresh: 0,
  });
  const [loaded, setLoaded] = useState({ key: initialKey, data: initialData });
  const queryKey = JSON.stringify({ ...apiQuery, refresh: refreshCount });
  const loading = loaded.key !== queryKey;

  useEffect(() => {
    if (loaded.key === queryKey) {
      return;
    }

    // A newer query cancels this one, so a slow response can never overwrite fresher rows.
    let cancelled = false;
    clientApi.employees
      .list(apiQuery)
      .then((data) => {
        if (!cancelled) {
          setLoaded({ key: queryKey, data });
        }
      })
      .catch((error: Error) => {
        if (!cancelled) {
          notify("error", `Could not load employees. ${error.message}`);
          setLoaded((current) => ({ ...current, key: queryKey }));
        }
      });

    return () => {
      cancelled = true;
    };
  }, [apiQuery, queryKey, loaded.key, notify]);

  const resetToFirstPage = () => setPaging((current) => ({ ...current, skip: 0 }));

  const onPageChange = (event: GridPageChangeEvent) => setPaging({ skip: event.page.skip, take: event.page.take });

  const onSortChange = (event: GridSortChangeEvent) => {
    setSort(event.sort);
    resetToFirstPage();
  };

  const departmentOptions = useMemo(() => departments.map(({ id, name }) => ({ id, name })), [departments]);

  // Kendo's own responsive pager swaps its page buttons for a page-number input by measuring the
  // viewport before hydration finishes, which never matches the server HTML on a phone. Choosing
  // the layout here instead (server and first client render both assume "narrow") hydrates cleanly.
  const isDesktop = useMediaQuery("(min-width: 992px)");
  const pager = useMemo(
    () => ({
      type: isDesktop ? ("numeric" as const) : ("input" as const),
      responsive: false,
      buttonCount: 5,
      info: isDesktop,
      pageSizes: [10, 20, 50],
    }),
    [isDesktop],
  );

  // Re-query the grid, and re-run the server component so the page-level headline count is fresh too.
  const refreshAfterChange = () => {
    setRefreshCount((count) => count + 1);
    router.refresh();
  };

  const onSaved = (result: "created" | "updated") => {
    setEditing(null);
    notify("success", result === "created" ? "Employee added." : "Employee updated.");
    refreshAfterChange();
  };

  const confirmDelete = async () => {
    if (!deleting) {
      return;
    }

    setDeleteBusy(true);
    try {
      await clientApi.employees.remove(deleting.id);
      notify("success", `${deleting.firstName} ${deleting.lastName} was deleted.`);
      // Deleting the only row on the last page would leave an empty page, so step back one page.
      if (loaded.data.items.length === 1 && paging.skip > 0) {
        setPaging((current) => ({ ...current, skip: Math.max(0, current.skip - current.take) }));
      }
      setDeleting(null);
      refreshAfterChange();
    } catch (error) {
      notify("error", error instanceof ApiError ? error.message : "Could not delete the employee.");
    } finally {
      setDeleteBusy(false);
    }
  };

  return (
    <RowActionsContext value={{ onEdit: setEditing, onDelete: setDeleting }}>
      <div className="toolbar">
        <Input
          className="toolbar__search"
          value={searchText}
          placeholder="Search name, email or job title"
          aria-label="Search employees"
          onChange={(event) => {
            setSearchText(event.value);
            resetToFirstPage();
          }}
        />
        <DropDownList
          className="toolbar__filter"
          data={departmentOptions}
          textField="name"
          dataItemKey="id"
          defaultItem={{ id: null, name: "All departments" }}
          value={departmentOptions.find((option) => option.id === departmentId) ?? null}
          aria-label="Filter by department"
          onChange={(event) => {
            setDepartmentId(event.value?.id ?? null);
            resetToFirstPage();
          }}
        />
        <DropDownList
          className="toolbar__filter"
          data={statusOptions}
          textField="label"
          dataItemKey="value"
          defaultItem={{ value: "all", label: "All statuses" }}
          value={statusOptions.find((option) => option.value === status) ?? null}
          aria-label="Filter by status"
          onChange={(event) => {
            setStatus((event.value?.value ?? "all") as StatusFilter);
            resetToFirstPage();
          }}
        />
        <Button className="toolbar__action" svgIcon={plusIcon} themeColor="primary" onClick={() => setEditing("new")}>
          Add employee
        </Button>
      </div>

      <div className={`data-panel${loading ? " data-panel--loading" : ""}`}>
        <Grid
          data={loaded.data.items}
          total={loaded.data.total}
          skip={paging.skip}
          take={paging.take}
          dataItemKey="id"
          pageable={pager}
          sortable={{ mode: "single", allowUnsort: false }}
          sort={sort}
          onPageChange={onPageChange}
          onSortChange={onSortChange}
        >
          <GridColumn field="lastName" title="Name" width="190px" cells={{ data: NameCell }} />
          <GridColumn field="email" title="Email" width="270px" />
          <GridColumn field="jobTitle" title="Job title" width="190px" />
          <GridColumn field="departmentName" title="Department" width="170px" />
          <GridColumn field="hireDate" title="Hired" width="130px" cells={{ data: HireDateCell }} />
          <GridColumn field="salary" title="Salary" width="120px" cells={{ data: SalaryCell }} />
          <GridColumn field="isActive" title="Status" width="110px" cells={{ data: StatusCell }} />
          <GridColumn title="Actions" width="110px" sortable={false} cells={{ data: ActionsCell }} />
        </Grid>
        {loading && <Loader className="data-panel__loader" size="large" type="converging-spinner" />}
      </div>

      {editing && (
        <EmployeeFormDialog
          employee={editing === "new" ? null : editing}
          departments={departments}
          onClose={() => setEditing(null)}
          onSaved={onSaved}
        />
      )}

      {deleting && (
        <ConfirmDialog
          title="Delete employee"
          message={
            <>
              Delete <strong>{deleting.firstName} {deleting.lastName}</strong>? Their shifts will be removed too. This can&apos;t be undone.
            </>
          }
          busy={deleteBusy}
          onConfirm={confirmDelete}
          onCancel={() => setDeleting(null)}
        />
      )}
    </RowActionsContext>
  );
}
