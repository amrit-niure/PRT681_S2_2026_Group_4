import { createApi } from "./createApi";

const apiUrl = process.env.API_URL ?? "http://localhost:5280";

/**
 * API client for Server Components. It calls the .NET API directly during server-side rendering,
 * and `no-store` makes every request render fresh data instead of a build-time snapshot.
 */
export const serverApi = createApi(`${apiUrl}/api`, { cache: "no-store" });
