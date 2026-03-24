# Build stage
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
ARG BUILD_VERSION
ARG BUILD_DATE
WORKDIR /src

# Copy solution and project files
COPY ["DigitalStokvel.API/DigitalStokvel.API.csproj", "DigitalStokvel.API/"]
COPY ["DigitalStokvel.Core/DigitalStokvel.Core.csproj", "DigitalStokvel.Core/"]
COPY ["DigitalStokvel.Infrastructure/DigitalStokvel.Infrastructure.csproj", "DigitalStokvel.Infrastructure/"]
COPY ["DigitalStokvel.Services/DigitalStokvel.Services.csproj", "DigitalStokvel.Services/"]
COPY ["DigitalStokvel.USSD/DigitalStokvel.USSD.csproj", "DigitalStokvel.USSD/"]

# Restore dependencies
RUN dotnet restore "DigitalStokvel.API/DigitalStokvel.API.csproj"

# Copy source code
COPY . .

# Build and publish
WORKDIR "/src/DigitalStokvel.API"
RUN dotnet build "DigitalStokvel.API.csproj" -c Release -o /app/build
RUN dotnet publish "DigitalStokvel.API.csproj" -c Release -o /app/publish /p:UseAppHost=false

# Runtime stage
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime
WORKDIR /app

# Create non-root user for security
RUN groupadd -r appuser && useradd -r -g appuser appuser

# Copy published app
COPY --from=build /app/publish .

# Set ownership
RUN chown -R appuser:appuser /app

# Switch to non-root user
USER appuser

# Expose port
EXPOSE 8080

# Health check
HEALTHCHECK --interval=30s --timeout=3s --start-period=10s --retries=3 \
  CMD curl --fail http://localhost:8080/health || exit 1

# Labels
LABEL org.opencontainers.image.title="Digital Stokvel API"
LABEL org.opencontainers.image.description="Backend API for Digital Stokvel Banking Platform"
LABEL org.opencontainers.image.version="${BUILD_VERSION}"
LABEL org.opencontainers.image.created="${BUILD_DATE}"

# Entry point
ENTRYPOINT ["dotnet", "DigitalStokvel.API.dll"]
