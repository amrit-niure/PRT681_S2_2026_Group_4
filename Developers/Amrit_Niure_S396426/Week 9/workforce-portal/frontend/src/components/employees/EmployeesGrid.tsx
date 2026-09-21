"use client";

import { useEffect, useMemo, useState } from "react";
import { Grid, GridColumn, type GridCustomCellProps, type GridPageChangeEvent, type GridSortChangeEvent } from "@progress/kendo-react-grid";
import { Input } from "@progress/kendo-react-inputs";
import { DropDownList } from "@progress/kendo-react-dropdowns";
import { Loader } from "@progress/kendo-react-indicators";
import type { SortDescriptor } from "@progress/kendo-data-query";
import { clientApi } from "@/lib/api/client";
import type { Department, Employee, EmployeeQuery, PagedResult } from "@/lib/api/types";
import { formatCurrency, formatDate } from "@/lib/format";
import { useDebouncedValue } from "@/lib/useDebouncedValue";
import { useToast } from "@/components/Toaster";
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
  });
  const [loaded, setLoaded] = useState({ key: initialKey, data: initialData });
  const queryKey = JSON.stringify(apiQuery);
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

  return (
    <>
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
      </div>

      <div className={`data-panel${loading ? " data-panel--loading" : ""}`}>
        <Grid
          data={loaded.data.items}
          total={loaded.data.total}
          skip={paging.skip}
          take={paging.take}
          dataItemKey="id"
          pageable={{ pageSizes: [10, 20, 50], buttonCount: 5, info: true }}
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
        </Grid>
        {loading && <Loader className="data-panel__loader" size="large" type="converging-spinner" />}
      </div>
    </>
  );
}
