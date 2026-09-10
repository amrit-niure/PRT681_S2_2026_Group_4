# Containerising TodoApi & deploying to Azure App Service

## What's in the image

- Multi-stage build: `dotnet/sdk:10.0` compiles, `dotnet/aspnet:10.0` runs.
- Kestrel listens on **port 8080** (`ASPNETCORE_HTTP_PORTS=8080`) — the port Azure
  App Service for Linux containers probes by default.
- EF Core migrations run automatically at startup (`db.Database.Migrate()` in `Program.cs`).
- SQLite database defaults to `/home/data/todo.db`. `/home` is the one path Azure
  App Service persists across restarts/redeploys (needs `WEBSITES_ENABLE_APP_SERVICE_STORAGE=true`).

## Run locally

```bash
cd backend
docker build -t todoapi:local .

# DB stored in an anonymous volume so it survives container restarts
docker run --rm -p 8080:8080 -v todoapi-data:/home/data todoapi:local
# GET http://localhost:8080/api/todoitems
```

Override config with `-e` (double underscore = nested key):

```bash
docker run --rm -p 8080:8080 \
  -e ConnectionStrings__Default="Data Source=/home/data/todo.db" \
  -e Cors__AllowedOrigins__0="https://my-frontend.example.com" \
  todoapi:local
```

## Deploy to Azure App Service (Azure CLI)

Set names once:

```bash
RG=prt681-rg
LOCATION=australiaeast
ACR=prt681acr$RANDOM          # must be globally unique, lowercase alphanumeric
PLAN=prt681-plan
APP=prt681-todoapi-$RANDOM    # becomes https://<APP>.azurewebsites.net
```

### 1. Resource group + container registry

```bash
az group create -n $RG -l $LOCATION

az acr create -n $ACR -g $RG --sku Basic --admin-enabled true
```

### 2. Build the image in ACR (no local push needed)

```bash
az acr build -r $ACR -t todoapi:latest ./backend
```

### 3. App Service plan (Linux) + web app

```bash
az appservice plan create -n $PLAN -g $RG --is-linux --sku B1

az webapp create -n $APP -g $RG -p $PLAN \
  --container-image-name "$ACR.azurecr.io/todoapi:latest"
```

### 4. Let the web app pull from ACR

```bash
az webapp config container set -n $APP -g $RG \
  --container-image-name "$ACR.azurecr.io/todoapi:latest" \
  --container-registry-url "https://$ACR.azurecr.io" \
  --container-registry-user "$(az acr credential show -n $ACR --query username -o tsv)" \
  --container-registry-password "$(az acr credential show -n $ACR --query 'passwords[0].value' -o tsv)"
```

### 5. App settings

```bash
az webapp config appsettings set -n $APP -g $RG --settings \
  WEBSITES_PORT=8080 \
  WEBSITES_ENABLE_APP_SERVICE_STORAGE=true \
  ASPNETCORE_ENVIRONMENT=Production \
  ConnectionStrings__Default="Data Source=/home/data/todo.db" \
  Cors__AllowedOrigins__0="https://<your-frontend-host>"
```

### 6. Restart & check

```bash
az webapp restart -n $APP -g $RG
az webapp log tail -n $APP -g $RG          # watch startup + migrations
curl https://$APP.azurewebsites.net/api/todoitems
```

## Redeploying a new version

```bash
az acr build -r $ACR -t todoapi:latest ./backend
az webapp restart -n $APP -g $RG
```

(Or enable continuous deployment: `az webapp deployment container config -n $APP -g $RG --enable-cd true`
and wire the ACR webhook it prints.)

## Notes

- **SQLite is single-file / single-instance.** Keep the plan at one instance (no scale-out).
  For real multi-instance hosting, move to Azure SQL / PostgreSQL and swap
  `UseSqlite` for the matching provider.
- The `SQLitePCLRaw` NU1903 restore warning comes from the EF Core SQLite package
  version pinned in `TodoApi.csproj`; bump `Microsoft.EntityFrameworkCore.Sqlite`
  when a patched release is available.
- HTTPS is terminated by App Service in front of the container; the app receives
  plain HTTP on 8080, which is expected.
