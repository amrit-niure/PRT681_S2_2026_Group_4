# Full-stack Todo

Week 2 practice for PRT681: the console CRUD app rebuilt as an ASP.NET Core Web API
with Entity Framework Core, plus a minimal React frontend that lists, adds, toggles
and deletes tasks by calling the API.

```
todo-fullstack/
├── backend/            ASP.NET Core Web API (.NET 10)
│   └── TodoApi/        controllers, EF Core DbContext, migrations
└── frontend/           React + Vite + TypeScript
    └── src/            api client, components, App
```

## Backend — TodoApi

- **Stack:** ASP.NET Core Web API (controllers), EF Core 10, SQLite.
- **Data:** `TodoItem` entity → `TodoDbContext` → `TodoItems` table. The `InitialCreate`
  migration is applied automatically on startup.
- **Endpoints:** `GET/POST/PUT/DELETE /api/todoitems` (`GET /api/todoitems/{id}` too).
- **CORS:** a `frontend` policy allows the Vite dev server
  (`Cors:AllowedOrigins` in `appsettings.json`, default `http://localhost:5173`).

### Run

```bash
cd backend/TodoApi
dotnet run --launch-profile http      # http://localhost:5258, OpenAPI at /openapi/v1.json
```

### Migrations

```bash
cd backend
dotnet tool restore                                   # installs the pinned dotnet-ef
dotnet dotnet-ef migrations add <Name> --project TodoApi
dotnet dotnet-ef database update --project TodoApi
```

The SQLite file (`todo.db`) is created next to the project and is git-ignored.

## Frontend

- **Stack:** React 19, Vite, TypeScript.
- **API base URL:** `VITE_API_URL` in `.env` (see `.env.example`), default
  `http://localhost:5258`.
- **Structure:** `src/api/todos.ts` (fetch wrapper), `src/components/AddTodoForm.tsx`,
  `src/components/TodoList.tsx`, `src/App.tsx` (state + effects).

### Run

```bash
cd frontend
npm install
npm run dev                            # http://localhost:5173
```

Start the backend first, then the frontend, and open http://localhost:5173.
