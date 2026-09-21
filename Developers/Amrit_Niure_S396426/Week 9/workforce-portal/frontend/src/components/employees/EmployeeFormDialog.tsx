"use client";

import { useMemo, useState } from "react";
import { Dialog } from "@progress/kendo-react-dialogs";
import { Field, Form, FormElement } from "@progress/kendo-react-form";
import { Button } from "@progress/kendo-react-buttons";
import { clientApi } from "@/lib/api/client";
import type { Department, Employee, EmployeeInput } from "@/lib/api/types";
import { toFormErrors, withoutError } from "@/lib/formErrors";
import { fromDateOnly, toDateOnly } from "@/lib/format";
import { combine, email, maxLength, minLength, notBefore, notInFuture, range, required } from "@/lib/validators";
import {
  FormDatePicker,
  FormDropDown,
  FormNumericInput,
  FormSwitch,
  FormTextInput,
} from "@/components/forms/FormFields";

interface EmployeeFormValues {
  firstName: string;
  lastName: string;
  email: string;
  jobTitle: string;
  departmentId: number | null;
  hireDate: Date | null;
  salary: number | null;
  isActive: boolean;
}

// Same floor as SaveEmployeeRequest.MinHireDate on the API.
const MIN_HIRE_DATE = new Date(1950, 0, 1);

const FIELD_NAMES =["firstName", "lastName", "email", "jobTitle", "departmentId", "hireDate", "salary", "isActive"];

const blankValues: EmployeeFormValues = {
  firstName: "",
  lastName: "",
  email: "",
  jobTitle: "",
  departmentId: null,
  hireDate: null,
  salary: null,
  isActive: true,
};

const toFormValues = (employee: Employee): EmployeeFormValues => ({
  firstName: employee.firstName,
  lastName: employee.lastName,
  email: employee.email,
  jobTitle: employee.jobTitle,
  departmentId: employee.departmentId,
  hireDate: fromDateOnly(employee.hireDate),
  salary: employee.salary,
  isActive: employee.isActive,
});

interface EmployeeFormDialogProps {
  /** The employee being edited, or null to add a new one. */
  employee: Employee | null;
  departments: Department[];
  onClose: () => void;
  onSaved: (result: "created" | "updated") => void;
}

export function EmployeeFormDialog({ employee, departments, onClose, onSaved }: EmployeeFormDialogProps) {
  const [saving, setSaving] = useState(false);
  // Messages from the API, keyed by field: shown under the input and cleared as soon as the user edits it.
  const [serverErrors, setServerErrors] = useState<Record<string, string>>({});
  const [formError, setFormError] = useState<string | null>(null);

  const departmentOptions = useMemo(
    () => departments.map((department) => ({ value: department.id, label: department.name })),
    [departments],
  );

  const submit = async (values: { [name: string]: unknown }) => {
    const form = values as unknown as EmployeeFormValues;
    const input: EmployeeInput = {
      firstName: form.firstName.trim(),
      lastName: form.lastName.trim(),
      email: form.email.trim(),
      jobTitle: form.jobTitle.trim(),
      departmentId: form.departmentId!,
      hireDate: toDateOnly(form.hireDate!),
      salary: form.salary!,
      isActive: form.isActive,
    };

    setSaving(true);
    setFormError(null);
    setServerErrors({});
    try {
      if (employee) {
        await clientApi.employees.update(employee.id, input);
      } else {
        await clientApi.employees.create(input);
      }
      onSaved(employee ? "updated" : "created");
    } catch (error) {
      const { fieldErrors, formError } = toFormErrors(error, FIELD_NAMES);
      setServerErrors(fieldErrors);
      setFormError(formError);
    } finally {
      setSaving(false);
    }
  };

  return (
    <Dialog title={employee ? `Edit ${employee.firstName} ${employee.lastName}` : "Add employee"} width={680} onClose={onClose}>
      <Form
        initialValues={employee ? toFormValues(employee) : blankValues}
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

            <div className="form-grid">
              <Field
                name="firstName"
                label="First name"
                component={FormTextInput}
                validator={combine(required("First name is required."), maxLength(100))}
              />
              <Field
                name="lastName"
                label="Last name"
                component={FormTextInput}
                validator={combine(required("Last name is required."), maxLength(100))}
              />
              <div className="form-grid__full">
                <Field
                  name="email"
                  label="Email"
                  type="email"
                  component={FormTextInput}
                  validator={combine(required("Email is required."), email, maxLength(254))}
                />
              </div>
              <Field
                name="jobTitle"
                label="Job title"
                component={FormTextInput}
                validator={combine(required("Job title is required."), minLength(2), maxLength(100))}
              />
              <Field
                name="departmentId"
                label="Department"
                placeholder="Select a department"
                component={FormDropDown}
                options={departmentOptions}
                validator={required("Select a department.")}
              />
              <Field
                name="hireDate"
                label="Hire date"
                component={FormDatePicker}
                min={MIN_HIRE_DATE}
                validator={combine(
                  required("Hire date is required."),
                  notBefore(MIN_HIRE_DATE, "Hire date can't be before 1950."),
                  notInFuture,
                )}
              />
              <Field
                name="salary"
                label="Annual salary (AUD)"
                component={FormNumericInput}
                format="n0"
                min={0}
                max={1_000_000}
                step={1000}
                validator={combine(required("Salary is required."), range(0, 1_000_000))}
              />
              <Field name="isActive" label="Active employee" component={FormSwitch} onLabel="Yes" offLabel="No" />
            </div>

            <div className="form-actions">
              <Button type="button" onClick={onClose} disabled={saving}>
                Cancel
              </Button>
              <Button type="submit" themeColor="primary" disabled={saving}>
                {saving ? "Saving..." : employee ? "Save changes" : "Add employee"}
              </Button>
            </div>
          </FormElement>
        )}
      />
    </Dialog>
  );
}
