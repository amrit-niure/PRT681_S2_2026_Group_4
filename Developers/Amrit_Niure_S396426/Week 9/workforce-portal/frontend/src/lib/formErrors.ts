import { ApiError } from "./api/http";

export interface FormErrors {
  /** First message per field, for <Form errors={...}>. Only fields the form actually has. */
  fieldErrors: Record<string, string>;
  /** A general message, set only when nothing can be pinned on a specific field. */
  formError: string | null;
}

/**
 * Turns a failed save into what a Kendo form can show: per-field messages from the API's validation
 * response, or a single general message (e.g. the server was unreachable, or a 409 conflict).
 */
export function toFormErrors(error: unknown, fieldNames: string[]): FormErrors {
  if (!(error instanceof ApiError)) {
    return { fieldErrors: {}, formError: "Could not reach the server. Check your connection and try again." };
  }

  const fieldErrors = Object.fromEntries(
    Object.entries(error.fieldErrors)
      .filter(([field]) => fieldNames.includes(field))
      .map(([field, messages]) => [field, messages[0]]),
  );

  return { fieldErrors, formError: Object.keys(fieldErrors).length > 0 ? null : error.message };
}

/** Removes one field's server error, used when the user edits that field. */
export const withoutError = (errors: Record<string, string>, fieldName: string) =>
  Object.fromEntries(Object.entries(errors).filter(([field]) => field !== fieldName));
