import { createApi } from "./createApi";

/** API client for Client Components: same-origin requests that Next.js proxies to the .NET API. */
export const clientApi = createApi("/backend");
