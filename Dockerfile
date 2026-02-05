# ===========================================
# Forma - Multi-stage Dockerfile
# ===========================================

# ---------------------------------
# Stage 1: Build Frontend
# ---------------------------------
FROM node:22-alpine AS frontend-build

WORKDIR /app/frontend

# Install pnpm
RUN corepack enable && corepack prepare pnpm@latest --activate

# Copy frontend package files
COPY src/Forma.Web/package.json src/Forma.Web/pnpm-lock.yaml* ./

# Install dependencies
RUN pnpm install --frozen-lockfile || pnpm install

# Copy frontend source
COPY src/Forma.Web/ ./

# Build frontend
RUN pnpm build

# ---------------------------------
# Stage 2: Build Backend
# ---------------------------------
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS backend-build

WORKDIR /src

# Copy solution and project files
COPY Forma.sln ./
COPY src/Forma.Domain/Forma.Domain.csproj src/Forma.Domain/
COPY src/Forma.Shared/Forma.Shared.csproj src/Forma.Shared/
COPY src/Forma.Application/Forma.Application.csproj src/Forma.Application/
COPY src/Forma.Infrastructure/Forma.Infrastructure.csproj src/Forma.Infrastructure/
COPY src/Forma.API/Forma.API.csproj src/Forma.API/

# Restore dependencies
RUN dotnet restore

# Copy all source code
COPY src/ src/

# Build and publish
RUN dotnet publish src/Forma.API/Forma.API.csproj -c Release -o /app/publish --no-restore

# ---------------------------------
# Stage 3: Runtime
# ---------------------------------
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime

WORKDIR /app

# Create non-root user for security
RUN addgroup --system --gid 1001 forma && \
    adduser --system --uid 1001 --ingroup forma forma

# Copy published backend
COPY --from=backend-build /app/publish ./

# Copy frontend build to wwwroot
COPY --from=frontend-build /app/frontend/dist ./wwwroot

# Create directories for uploads and data
RUN mkdir -p /app/uploads /app/data && \
    chown -R forma:forma /app

# Switch to non-root user
USER forma

# Expose port
EXPOSE 8080

# Set environment variables
ENV ASPNETCORE_URLS=http://+:8080
ENV ASPNETCORE_ENVIRONMENT=Production

# Health check
HEALTHCHECK --interval=30s --timeout=10s --start-period=5s --retries=3 \
    CMD wget --no-verbose --tries=1 --spider http://localhost:8080/api/health || exit 1

# Start application
ENTRYPOINT ["dotnet", "Forma.API.dll"]
