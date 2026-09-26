# Ticketing (full stack)

A support-ticket system: raise tickets, comment, assign and move them through
Open → In progress → Resolved → Closed. Requesters get an email when a ticket is logged and
whenever its status changes.

| Layer | Tech |
|---|---|
| Frontend | React 19, Vite, Tailwind, shadcn/ui |
| API | ASP.NET Core (.NET 10), EF Core, ASP.NET Core Identity (bearer tokens) |
| Database | PostgreSQL |
| Logging | Serilog `ILogger<T>` → console + **Seq** (structured properties) |
| Exceptions | **ELMAH** (`/elmah`) and **Exceptionless** (optional) |
| Email | MailKit over the **Resend SMTP** relay |
| Async dispatch | **Temporal** workflow (`TicketEmailWorkflow`) with retries |
| Orchestration | **.NET Aspire** *or* plain **Docker Compose**, your choice |

## Option 1: Aspire

```bash
cd backend
dotnet run --project TicketingApi.AppHost --launch-profile http
```

API as a debuggable project, frontend as the Vite dev server, everything else in containers.
To run the API and frontend from their **Dockerfiles** under Aspire as well:

```bash
dotnet run --project TicketingApi.AppHost --launch-profile http -- --Containers:Enabled=true
```

Secrets go in the AppHost's user-secrets and are passed to the API:

```bash
dotnet user-secrets set Smtp:Password re_your_resend_key --project TicketingApi.AppHost
dotnet user-secrets set Smtp:FromAddress you@your-verified-domain.com --project TicketingApi.AppHost
dotnet user-secrets set Exceptionless:ApiKey <key> --project TicketingApi.AppHost   # optional
```

## Option 2: Docker Compose (no Aspire)

```bash
cp .env.example .env      # set POSTGRES_PASSWORD and Smtp__Password (your Resend API key)
docker compose up --build
```

## Where things are

| | Aspire | Compose |
|---|---|---|
| Frontend | http://localhost:5177 | http://localhost:5177 |
| API + Scalar docs | http://localhost:5260 | http://localhost:8080 |
| Seq | http://localhost:5342 | http://localhost:8081 |
| Temporal UI | http://localhost:8233 | http://localhost:8233 |
| ELMAH | `<api>/elmah` | `<api>/elmah` |
| Health | `<api>/health`, `<api>/alive` | same |

## Verifying it works

1. **Health**: `curl <api>/alive` and `curl <api>/health` (the latter includes the database).
   Both images define a Docker `HEALTHCHECK`; `docker compose ps` shows `healthy`.
2. **Logs in Seq**: raise a ticket in the UI, open Seq and filter `TicketId = 1`. Each event carries
   structured properties (`TicketId`, `Application`, request path/status/elapsed).
3. **Exceptions**: with `Diagnostics:Enabled=true` (on in Development and in Compose), call
   `GET <api>/api/diagnostics/throw`. It appears in ELMAH (`<api>/elmah`) and, if
   `Exceptionless:ApiKey` is set, in Exceptionless.
4. **Email workflow**: raise a ticket, then open the Temporal UI. A `TicketEmailWorkflow` run
   completes; failures retry with backoff (5 attempts). Without `Smtp:Password` the send is logged
   and skipped, so the workflow still completes.

## Notes

- Resend SMTP: host `smtp.resend.com`, port 465 (implicit TLS), user `resend`, password = API key.
  The From address must be on a domain verified in Resend (`onboarding@resend.dev` only delivers to
  your own account email).
- The compose Seq runs with authentication off for local use. Set `SEQ_FIRSTRUN_ADMINPASSWORD`
  instead before exposing it.
- The API image targets `linux-musl` on Alpine and runs as a non-root user; mount volumes at
  `/app/dp-keys` and `/app/elmah-logs` to keep auth keys and error logs across restarts.
