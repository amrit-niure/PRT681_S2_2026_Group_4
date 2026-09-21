"use client";

import { createContext, useContext, useState } from "react";
import { useRouter } from "next/navigation";
import { Grid, GridColumn, type GridCustomCellProps } from "@progress/kendo-react-grid";
import { Button } from "@progress/kendo-react-buttons";
import { pencilIcon, plusIcon, trashIcon } from "@progress/kendo-svg-icons";
import { clientApi } from "@/lib/api/client";
import { ApiError } from "@/lib/api/http";
import type { Department } from "@/lib/api/types";
import { ConfirmDialog } from "@/components/ConfirmDialog";
import { useToast } from "@/components/Toaster";
import { DepartmentFormDialog } from "./DepartmentFormDialog";

// Grid cells are rendered by Kendo and can't take extra props, so the row buttons use context.
const RowActionsContext = createContext<{ onEdit: (department: Department) => void; onDelete: (department: Department) => void }>({
  onEdit: () => {},
  onDelete: () => {},
});

const ActionsCell = ({ tdProps, dataItem }: GridCustomCellProps) => {
  const { onEdit, onDelete } = useContext(RowActionsContext);
  const inUse = dataItem.employeeCount > 0;
  return (
    <td {...tdProps}>
      <Button
        svgIcon={pencilIcon}
        fillMode="flat"
        size="small"
        title={`Edit ${dataItem.name}`}
        aria-label={`Edit ${dataItem.name}`}
        onClick={() => onEdit(dataItem)}
      />
      <Button
        svgIcon={trashIcon}
        fillMode="flat"
        size="small"
        themeColor="error"
        disabled={inUse}
        title={inUse ? `Move or remove its ${dataItem.employeeCount} employees first` : `Delete ${dataItem.name}`}
        aria-label={`Delete ${dataItem.name}`}
        onClick={() => onDelete(dataItem)}
      />
    </td>
  );
};

/**
 * Small reference table, so the server component's list is used directly: after a change we ask Next.js
 * to re-render the page and this grid receives the fresh list as new props.
 */
export function DepartmentsGrid({ departments }: { departments: Department[] }) {
  const { notify } = useToast();
  const router = useRouter();

  const [editing, setEditing] = useState<Department | "new" | null>(null);
  const [deleting, setDeleting] = useState<Department | null>(null);
  const [deleteBusy, setDeleteBusy] = useState(false);

  const onSaved = (result: "created" | "updated") => {
    setEditing(null);
    notify("success", result === "created" ? "Department added." : "Department updated.");
    router.refresh();
  };

  const confirmDelete = async () => {
    if (!deleting) {
      return;
    }

    setDeleteBusy(true);
    try {
      await clientApi.departments.remove(deleting.id);
      notify("success", `${deleting.name} was deleted.`);
      setDeleting(null);
      router.refresh();
    } catch (error) {
      notify("error", error instanceof ApiError ? error.message : "Could not delete the department.");
    } finally {
      setDeleteBusy(false);
    }
  };

  return (
    <RowActionsContext value={{ onEdit: setEditing, onDelete: setDeleting }}>
      <div className="toolbar">
        <Button className="toolbar__action" svgIcon={plusIcon} themeColor="primary" onClick={() => setEditing("new")}>
          Add department
        </Button>
      </div>

      <div className="data-panel">
        <Grid data={departments} dataItemKey="id" sortable autoProcessData>
          <GridColumn field="name" title="Name" width="220px" />
          <GridColumn field="description" title="Description" />
          <GridColumn field="employeeCount" title="Employees" width="130px" />
          <GridColumn title="Actions" width="110px" sortable={false} cells={{ data: ActionsCell }} />
        </Grid>
      </div>

      {editing && (
        <DepartmentFormDialog department={editing === "new" ? null : editing} onClose={() => setEditing(null)} onSaved={onSaved} />
      )}

      {deleting && (
        <ConfirmDialog
          title="Delete department"
          message={
            <>
              Delete <strong>{deleting.name}</strong>? This can&apos;t be undone.
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
