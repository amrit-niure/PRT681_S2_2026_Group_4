FROM mcr.microsoft.com/dotnet/sdk:10.0-alpine AS build
WORKDIR /src

ARG TARGETARCH

COPY TicketingApi.ServiceDefaults/TicketingApi.ServiceDefaults.csproj TicketingApi.ServiceDefaults/
COPY TicketingApi/TicketingApi.csproj TicketingApi/
RUN RID=linux-musl-$([ "$TARGETARCH" = "arm64" ] && echo arm64 || echo x64) \
 && dotnet restore TicketingApi/TicketingApi.csproj -r $RID

COPY TicketingApi.ServiceDefaults/ TicketingApi.ServiceDefaults/
COPY TicketingApi/ TicketingApi/
WORKDIR /src/TicketingApi
RUN RID=linux-musl-$([ "$TARGETARCH" = "arm64" ] && echo arm64 || echo x64) \
 && dotnet publish -c Release -o /app/publish --no-restore -r $RID --self-contained false

FROM mcr.microsoft.com/dotnet/aspnet:10.0-alpine AS final
WORKDIR /app

RUN apk add --no-cache krb5-libs

RUN mkdir -p /app/dp-keys /app/elmah-logs && chown -R app:app /app/dp-keys /app/elmah-logs

COPY --from=build /app/publish .

USER app
ENV ASPNETCORE_HTTP_PORTS=8080
EXPOSE 8080

HEALTHCHECK --interval=15s --timeout=5s --start-period=30s --retries=5 \
  CMD wget -qO- http://localhost:8080/alive || exit 1

ENTRYPOINT ["dotnet", "TicketingApi.dll"]
