/** Field name -> messages, e.g. { email: ["An employee with this email already exists."] }. */
export type FieldErrors = Record<string, string[]>;

/**
 * Thrown for any non-2xx response. For ASP.NET Core validation failures (400 ProblemDetails) the
 * per-field messages are exposed in `fieldErrors` so forms can show them under the right input.
 */
export class ApiError extends Error {
  readonly status: number;
  readonly fieldErrors: FieldErrors;

  constructor(status: number, message: string, fieldErrors: FieldErrors = {}) {
    super(message);
    this.name = "ApiError";
    this.status = status;
    this.fieldErrors = fieldErrors;
  }
}

interface ProblemDetails {
  title?: string;
  detail?: string;
  errors?: Record<string, string[]>;
}

// The API reports fields as declared in C# ("HireDate"); the client uses camelCase ("hireDate").
const toCamelCase = (key: string) => key.charAt(0).toLowerCase() + key.slice(1);

async function toApiError(response: Response): Promise<ApiError> {
  let problem: ProblemDetails | undefined;
  try {
    problem = (await response.json()) as ProblemDetails;
  } catch {
    // Not JSON (e.g. a proxy error page); fall through to the generic message.
  }

  const fieldErrors: FieldErrors = {};
  for (const [key, messages] of Object.entries(problem?.errors ?? {})) {
    fieldErrors[toCamelCase(key)] = messages;
  }

  const message =
    problem?.detail ??
    (Object.keys(fieldErrors).length > 0 ? "Please fix the highlighted fields." : problem?.title) ??
    `Request failed (${response.status})`;

  return new ApiError(response.status, message, fieldErrors);
}

export async function request<T>(url: string, init?: RequestInit): Promise<T> {
  const response = await fetch(url, {
    ...init,
    headers: {
      Accept: "application/json",
      ...(init?.body ? { "Content-Type": "application/json" } : {}),
      ...init?.headers,
    },
  });

  if (!response.ok) {
    throw await toApiError(response);
  }

  // 204 No Content (PUT / DELETE) has no body to parse.
  return (response.status === 204 ? undefined : await response.json()) as T;
}

/** Builds "?a=1&b=2", skipping undefined/empty values. */
export function toQueryString(params: object): string {
  const search = new URLSearchParams();
  for (const [key, value] of Object.entries(params)) {
    if (value !== undefined && value !== null && value !== "") {
      search.set(key, String(value));
    }
  }
  const text = search.toString();
  return text ? `?${text}` : "";
}
