"use client";

import type { ReactNode } from "react";
import { FieldWrapper, type FieldRenderProps } from "@progress/kendo-react-form";
import { Error as FieldError, Hint, Label } from "@progress/kendo-react-labels";
import { Input, NumericTextBox, Switch, TextArea } from "@progress/kendo-react-inputs";
import { DatePicker } from "@progress/kendo-react-dateinputs";
import { DropDownList } from "@progress/kendo-react-dropdowns";

// Field components for Kendo's <Field component={...}>. Each one renders label + input + message the
// same way, and shows `validationMessage` for both client validators and errors the API returned
// (those arrive through <Form errors={...}>), so users get one consistent kind of feedback.

interface CommonProps extends FieldRenderProps {
  label: string;
  hint?: string;
  optional?: boolean;
  placeholder?: string;
}

function FieldShell({
  id,
  label,
  valid,
  optional,
  hint,
  error,
  children,
}: {
  id: string;
  label: string;
  valid: boolean;
  optional?: boolean;
  hint?: string;
  error?: string | null;
  children: ReactNode;
}) {
  return (
    <FieldWrapper>
      <Label editorId={id} editorValid={valid} optional={optional}>
        {label}
      </Label>
      <div className="k-form-field-wrap">
        {children}
        {error ? <FieldError id={`${id}-error`}>{error}</FieldError> : hint ? <Hint>{hint}</Hint> : null}
      </div>
    </FieldWrapper>
  );
}

// Errors are shown once the user has interacted with the field or tried to submit.
const visibleError = ({ touched, modified, visited, validationMessage }: FieldRenderProps) =>
  touched || modified || visited ? validationMessage : null;

export function FormTextInput(props: CommonProps & { type?: string }) {
  const { name, value, onChange, onBlur, onFocus, valid, label, hint, optional, placeholder, type } = props;
  const id = `field-${name}`;
  return (
    <FieldShell id={id} label={label} valid={valid} optional={optional} hint={hint} error={visibleError(props)}>
      <Input
        id={id}
        name={name}
        type={type}
        value={value ?? ""}
        valid={valid}
        placeholder={placeholder}
        onChange={onChange}
        onBlur={onBlur}
        onFocus={onFocus}
      />
    </FieldShell>
  );
}

export function FormTextArea(props: CommonProps & { rows?: number }) {
  const { name, value, onChange, onBlur, onFocus, valid, label, hint, optional, placeholder, rows = 3 } = props;
  const id = `field-${name}`;
  return (
    <FieldShell id={id} label={label} valid={valid} optional={optional} hint={hint} error={visibleError(props)}>
      <TextArea
        id={id}
        name={name}
        rows={rows}
        value={value ?? ""}
        valid={valid}
        placeholder={placeholder}
        onChange={onChange}
        onBlur={onBlur}
        onFocus={onFocus}
      />
    </FieldShell>
  );
}

export function FormNumericInput(props: CommonProps & { format?: string; min?: number; max?: number; step?: number }) {
  const { name, value, onChange, onBlur, onFocus, valid, label, hint, optional, format, min, max, step } = props;
  const id = `field-${name}`;
  return (
    <FieldShell id={id} label={label} valid={valid} optional={optional} hint={hint} error={visibleError(props)}>
      <NumericTextBox
        id={id}
        name={name}
        value={value ?? null}
        valid={valid}
        format={format}
        min={min}
        max={max}
        step={step}
        onChange={onChange}
        onBlur={onBlur}
        onFocus={onFocus}
      />
    </FieldShell>
  );
}

export function FormDatePicker(props: CommonProps & { max?: Date }) {
  const { name, value, onChange, onBlur, onFocus, valid, label, hint, optional, max } = props;
  const id = `field-${name}`;
  return (
    <FieldShell id={id} label={label} valid={valid} optional={optional} hint={hint} error={visibleError(props)}>
      <DatePicker
        id={id}
        name={name}
        value={value ?? null}
        valid={valid}
        max={max}
        format="dd/MM/yyyy"
        placeholder="dd/mm/yyyy"
        onChange={onChange}
        onBlur={onBlur}
        onFocus={onFocus}
      />
    </FieldShell>
  );
}

export interface SelectOption {
  value: number;
  label: string;
}

export function FormDropDown(props: CommonProps & { options: SelectOption[] }) {
  const { name, value, onChange, onBlur, onFocus, valid, label, hint, optional, placeholder, options } = props;
  const id = `field-${name}`;
  const selected = options.find((option) => option.value === value) ?? null;
  return (
    <FieldShell id={id} label={label} valid={valid} optional={optional} hint={hint} error={visibleError(props)}>
      <DropDownList
        id={id}
        name={name}
        data={options}
        textField="label"
        dataItemKey="value"
        value={selected}
        valid={valid}
        defaultItem={{ value: null, label: placeholder ?? "Select..." }}
        onChange={(event) => onChange({ value: event.value?.value ?? null })}
        onBlur={onBlur}
        onFocus={onFocus}
      />
    </FieldShell>
  );
}

export function FormSwitch(props: CommonProps & { onLabel?: string; offLabel?: string }) {
  const { name, value, onChange, onBlur, onFocus, valid, label, hint, onLabel = "Yes", offLabel = "No" } = props;
  const id = `field-${name}`;
  return (
    <FieldShell id={id} label={label} valid={valid} hint={hint} error={visibleError(props)}>
      <Switch
        id={id}
        name={name}
        checked={Boolean(value)}
        onLabel={onLabel}
        offLabel={offLabel}
        onChange={(event) => onChange({ value: event.value })}
        onBlur={onBlur}
        onFocus={onFocus}
      />
    </FieldShell>
  );
}
