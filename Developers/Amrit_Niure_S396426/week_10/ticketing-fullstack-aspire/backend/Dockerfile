# ---- build ----
FROM mcr.microsoft.com/dotnet/sdk:10.0-alpine AS build
WORKDIR /src

ARG TARGETARCH

# Restore first so this layer is cached until a csproj changes.
COPY TicketingApi.ServiceDefaults/TicketingApi.ServiceDefaults.csproj TicketingApi.ServiceDefaults/
COPY TicketingApi/TicketingApi.csproj TicketingApi/
# Publishing for one runtime (not "any") keeps only that platform's native libraries, e.g.
# Temporal's, instead of shipping every OS/arch (~200 MB saved).
RUN RID=linux-musl-$([ "$TARGETARCH" = "arm64" ] && echo arm64 || echo x64) \
 && dotnet restore TicketingApi/TicketingApi.csproj -r $RID

COPY TicketingApi.ServiceDefaults/ TicketingApi.ServiceDefaults/
COPY TicketingApi/ TicketingApi/
WORKDIR /src/TicketingApi
RUN RID=linux-musl-$([ "$TARGETARCH" = "arm64" ] && echo arm64 || echo x64) \
 && dotnet publish -c Release -o /app/publish --no-restore -r $RID --self-contained false

# ---- runtime (Alpine keeps the image small) ----
FROM mcr.microsoft.com/dotnet/aspnet:10.0-alpine AS final
WORKDIR /app

# Npgsql probes for Kerberos (GSSAPI) on connect; without this Alpine logs a load error.
RUN apk add --no-cache krb5-libs

# Writable state for the non-root "app" user: data-protection keys and ELMAH error logs.
# Mount volumes here so they survive container restarts.
RUN mkdir -p /app/dp-keys /app/elmah-logs && chown -R app:app /app/dp-keys /app/elmah-logs

COPY --from=build /app/publish .

USER app
ENV ASPNETCORE_HTTP_PORTS=8080
EXPOSE 8080

# busybox wget ships with Alpine, so no extra packages are needed. /alive only checks that the
# process responds; /health also covers the database.
HEALTHCHECK --interval=15s --timeout=5s --start-period=30s --retries=5 \
  CMD wget -qO- http://localhost:8080/alive || exit 1

ENTRYPOINT ["dotnet", "TicketingApi.dll"]
