# ---- Build stage: compile and publish using the full SDK image ----
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

# Copy only the project files first, then restore.
# This layer is cached and only re-runs when a .csproj changes — not on every code edit.
COPY src/CSharpApp.Api/CSharpApp.Api.csproj CSharpApp.Api/
COPY src/CSharpApp.Application/CSharpApp.Application.csproj CSharpApp.Application/
COPY src/CSharpApp.Core/CSharpApp.Core.csproj CSharpApp.Core/
COPY src/CSharpApp.Infrastructure/CSharpApp.Infrastructure.csproj CSharpApp.Infrastructure/
RUN dotnet restore CSharpApp.Api/CSharpApp.Api.csproj

# Copy the rest of the source and publish a Release build.
COPY src/ ./
RUN dotnet publish CSharpApp.Api/CSharpApp.Api.csproj -c Release -o /app/publish --no-restore

# ---- Runtime stage: small image with only the runtime + published app ----
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS final
WORKDIR /app
COPY --from=build /app/publish ./

# The aspnet image listens on port 8080 by default.
EXPOSE 8080
ENTRYPOINT ["dotnet", "CSharpApp.Api.dll"]