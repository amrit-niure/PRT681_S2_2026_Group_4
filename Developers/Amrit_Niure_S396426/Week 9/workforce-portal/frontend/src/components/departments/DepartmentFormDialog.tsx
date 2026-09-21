"use client";

import { useState } from "react";
import { Dialog } from "@progress/kendo-react-dialogs";
import { Field, Form, FormElement } from "@progress/kendo-react-form";
import { Button } from "@progress/kendo-react-buttons";
import { clientApi } from "@/lib/api/client";
import type { Department, DepartmentInput } from "@/lib/api/types";
import { toFormErrors, withoutError } from "@/lib/formErrors";
import { combine, maxLength, minLength, required } from "@/lib/validators";
import { FormTextArea, FormTextInput } from "@/components/forms/FormFields";

interface DepartmentFormValues {
  name: string;
  description: string;
}

const FIELD_NAMES = ["name", "description"];

interface DepartmentFormDialogProps {
  /** The department being edited, or null to add a new one. */
  department: Department | null;
  onClose: () => void;
  onSaved: (result: "created" | "updated") => void;
}

export function DepartmentFormDialog({ department, onClose, onSaved }: DepartmentFormDialogProps) {
  const [saving, setSaving] = useState(false);
  const [serverErrors, setServerErrors] = useState<Record<string, string>>({});
  const [formError, setFormError] = useState<string | null>(null);

  const submit = async (values: { [name: string]: unknown }) => {
    const form = values as unknown as DepartmentFormValues;
    const input: DepartmentInput = { name: form.name.trim(), description: form.description.trim() || null };

    setSaving(true);
    setFormError(null);
    setServerErrors({});
    try {
      if (department) {
        await clientApi.departments.update(department.id, input);
      } else {
        await clientApi.departments.create(input);
      }
      onSaved(department ? "updated" : "created");
    } catch (error) {
      const { fieldErrors, formError } = toFormErrors(error, FIELD_NAMES);
      setServerErrors(fieldErrors);
      setFormError(formError);
    } finally {
      setSaving(false);
    }
  };

  return (
    <Dialog title={department ? `Edit ${department.name}` : "Add department"} width={520} onClose={onClose}>
      <Form
        initialValues={{ name: department?.name ?? "", description: department?.description ?? "" }}
        errors={serverErrors}
        onChange={(fieldName) => setServerErrors((current) => withoutError(current, fieldName))}
        onSubmit={submit}
        render={() => (
          <FormElement style={{ maxWidth: "none" }}>
            {formError && (
              <div className="form-alert" role="alert">
                {formError}
              </div>
            )}

            <Field
              name="name"
              label="Name"
              component={FormTextInput}
              validator={combine(required("Name is required."), minLength(2), maxLength(100))}
            />
            <Field
              name="description"
              label="Description"
              optional
              hint="Up to 500 characters."
              component={FormTextArea}
              validator={maxLength(500)}
            />

            <div className="form-actions">
              <Button type="button" onClick={onClose} disabled={saving}>
                Cancel
              </Button>
              <Button type="submit" themeColor="primary" disabled={saving}>
                {saving ? "Saving..." : department ? "Save changes" : "Add department"}
              </Button>
            </div>
          </FormElement>
        )}
      />
    </Dialog>
  );
}
