# ── Stage 1: BUILD ──────────────────────────────────────────────────
# Use the .NET 10 SDK to compile the app
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

# Copy .csproj files first (Docker caches this layer — speeds up rebuilds)
COPY AiStudyPlanner.API/AiStudyPlanner.API.csproj                         AiStudyPlanner.API/
COPY AiStudyPlanner.Application/AiStudyPlanner.Application.csproj         AiStudyPlanner.Application/
COPY AiStudyPlanner.Domain/AiStudyPlanner.Domain.csproj                   AiStudyPlanner.Domain/
COPY AiStudyPlanner.Infrastructure/AiStudyPlanner.Infrastructure.csproj   AiStudyPlanner.Infrastructure/

# Restore NuGet packages
RUN dotnet restore AiStudyPlanner.API/AiStudyPlanner.API.csproj

# Copy all source code
COPY . .

# Publish in Release mode
RUN dotnet publish AiStudyPlanner.API/AiStudyPlanner.API.csproj -c Release -o /app/publish

# ── Stage 2: RUNTIME ─────────────────────────────────────────────────
# Use only the ASP.NET runtime (smaller image — no SDK needed)
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime
WORKDIR /app

# Copy only the compiled output from Stage 1
COPY --from=build /app/publish .

# The app listens on port 8080
EXPOSE 8080

# Start the app
ENTRYPOINT ["dotnet", "AiStudyPlanner.API.dll"]
