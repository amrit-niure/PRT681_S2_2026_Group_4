import type { FieldValidatorType } from "@progress/kendo-react-form";

// Client-side rules. Each mirrors a DataAnnotations rule on the matching backend request DTO, so a
// user sees the message immediately instead of waiting for the API to answer with a 400.
// The API stays the source of truth: it re-checks everything and can add rules only it can enforce
// (unique email, department exists, shift overlaps), which the forms show via server field errors.

const isEmpty = (value: unknown) =>
  value === undefined || value === null || (typeof value === "string" && value.trim() === "");

export const required =
  (message = "This field is required."): FieldValidatorType =>
  (value) =>
    isEmpty(value) ? message : undefined;

export const minLength =
  (min: number): FieldValidatorType =>
  (value) =>
    typeof value === "string" && value.trim().length > 0 && value.trim().length < min
      ? `Enter at least ${min} characters.`
      : undefined;

export const maxLength =
  (max: number): FieldValidatorType =>
  (value) =>
    typeof value === "string" && value.trim().length > max ? `Enter no more than ${max} characters.` : undefined;

const EMAIL_PATTERN = /^[^\s@]+@[^\s@]+\.[^\s@]+$/;

export const email: FieldValidatorType = (value) =>
  typeof value === "string" && value.trim().length > 0 && !EMAIL_PATTERN.test(value.trim())
    ? "Enter a valid email address."
    : undefined;

export const range =
  (min: number, max: number): FieldValidatorType =>
  (value) =>
    typeof value === "number" && (value < min || value > max) ? `Enter a value between ${min} and ${max}.` : undefined;

/** A date that must not be after today (end of the local day). */
export const notInFuture: FieldValidatorType = (value) => {
  if (!(value instanceof Date)) {
    return undefined;
  }
  const endOfToday = new Date();
  endOfToday.setHours(23, 59, 59, 999);
  return value > endOfToday ? "Date can't be in the future." : undefined;
};

/** Runs validators in order and returns the first message, so each field shows one clear error. */
export const combine =
  (...validators: FieldValidatorType[]): FieldValidatorType =>
  (value, valueGetter, fieldProps) => {
    for (const validate of validators) {
      const message = validate(value, valueGetter, fieldProps);
      if (message) {
        return message;
      }
    }
    return undefined;
  };
