FROM mcr.microsoft.com/dotnet/sdk:10.0@sha256:2fa828c68761b1b8c23d7662dc134421b9d3b59fe1425fdbc80804e390cdb24d AS build
WORKDIR /src

COPY Identity.Domain/Identity.Domain.csproj Identity.Domain/
COPY Identity.Application/Identity.Application.csproj Identity.Application/
COPY Identity.Infrastructure/Identity.Infrastructure.csproj Identity.Infrastructure/
COPY Identity.Api/Identity.Api.csproj Identity.Api/
RUN dotnet restore Identity.Api/Identity.Api.csproj

COPY . .
RUN dotnet publish Identity.Api/Identity.Api.csproj -c Release -o /app/publish --no-restore

FROM mcr.microsoft.com/dotnet/aspnet:10.0@sha256:6a94333d37514e385650a3c81a55e5350b67253dbe136e9cf17e499c35606a8c AS final
WORKDIR /app
COPY --from=build /app/publish .

# Built-in non-root user baked into the base image (UID exposed via $APP_UID) — inert unless
# explicitly switched to, so the container ran as root without this.
USER $APP_UID

EXPOSE 8080
ENTRYPOINT ["dotnet", "Identity.Api.dll"]
