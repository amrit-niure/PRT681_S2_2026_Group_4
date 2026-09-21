// Shared by the server page (first render) and the client grid, so both start from the same query
// and the grid can reuse the server-rendered rows instead of fetching again on mount.
// Deliberately not in a "use client" file: a server component can only read plain values from a
// module that isn't a client boundary.

export const EMPLOYEE_PAGE_SIZE = 10;

export const EMPLOYEE_DEFAULT_SORT = { field: "lastName", dir: "asc" } as const;
