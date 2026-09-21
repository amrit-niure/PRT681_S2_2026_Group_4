"use client";

import type { ReactNode } from "react";
import { Dialog, DialogActionsBar } from "@progress/kendo-react-dialogs";
import { Button } from "@progress/kendo-react-buttons";

interface ConfirmDialogProps {
  title: string;
  message: ReactNode;
  confirmText?: string;
  /** Disables the buttons while the request is running. */
  busy?: boolean;
  onConfirm: () => void;
  onCancel: () => void;
}

/** Modal "are you sure?" prompt for destructive actions such as deleting a record. */
export function ConfirmDialog({ title, message, confirmText = "Delete", busy = false, onConfirm, onCancel }: ConfirmDialogProps) {
  return (
    <Dialog title={title} width={420} onClose={busy ? undefined : onCancel}>
      <p style={{ margin: 0 }}>{message}</p>
      <DialogActionsBar>
        <Button onClick={onCancel} disabled={busy}>
          Cancel
        </Button>
        <Button themeColor="error" onClick={onConfirm} disabled={busy}>
          {busy ? "Working..." : confirmText}
        </Button>
      </DialogActionsBar>
    </Dialog>
  );
}
