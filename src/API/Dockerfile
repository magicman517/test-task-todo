FROM node:22-alpine AS web-build
WORKDIR /web
RUN corepack enable && corepack prepare pnpm@11.22.0 --activate
COPY src/Web/package.json src/Web/pnpm-lock.yaml* src/Web/pnpm-workspace.yaml* ./
RUN pnpm install --frozen-lockfile
COPY src/Web/ .
RUN pnpm run build

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS base
USER $APP_UID
WORKDIR /app
EXPOSE 8080
EXPOSE 8081

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["src/API/API.csproj", "src/API/"]
COPY ["aspire/ServiceDefaults/ServiceDefaults.csproj", "aspire/ServiceDefaults/"]
RUN dotnet restore "src/API/API.csproj"
COPY . .
WORKDIR "/src/src/API"
RUN dotnet build "./API.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./API.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false
COPY --from=web-build /web/dist/Web/browser /app/publish/wwwroot

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "API.dll"]
