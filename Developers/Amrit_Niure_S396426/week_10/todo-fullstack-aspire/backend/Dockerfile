FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY TodoApi.ServiceDefaults/TodoApi.ServiceDefaults.csproj TodoApi.ServiceDefaults/
COPY TodoApi/TodoApi.csproj TodoApi/
RUN dotnet restore TodoApi/TodoApi.csproj

COPY TodoApi.ServiceDefaults/ TodoApi.ServiceDefaults/
COPY TodoApi/ TodoApi/
WORKDIR /src/TodoApi
RUN dotnet publish -c Release -o /app/publish --no-restore

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app
COPY --from=build /app/publish .

EXPOSE 8080
ENTRYPOINT ["sh", "-c", "ASPNETCORE_URLS=http://+:${PORT:-8080} exec dotnet TodoApi.dll"]
