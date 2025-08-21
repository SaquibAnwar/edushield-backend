# EduShield Backend

A .NET 8.0 Web API project with PostgreSQL database and Redis cache support.

## Prerequisites

- .NET 8.0 SDK
- Docker and Docker Compose
- Your favorite IDE (Visual Studio, VS Code, Rider, etc.)

## Getting Started

### Quick Start

Run the following command to start the entire application:

```bash
./run.sh
```

This script will:
1. Start PostgreSQL and Redis using Docker Compose
2. Restore NuGet packages
3. Build the project
4. Run database migrations
5. Start the API server

### Manual Setup

1. **Start Infrastructure**
   ```bash
   docker-compose up -d
   ```

2. **Run the API**
   ```bash
   cd src/Api/EduShield.Api
   dotnet restore
   dotnet build
   dotnet run
   ```

## API Endpoints

- **Health Check**: `GET /api/v1/health`
- **Health Ping**: `GET /api/v1/health/ping`
- **Swagger UI**: `/swagger`
- **Health Checks**: `/health`

## Services

- **API**: https://localhost:5001 (HTTPS) or http://localhost:5000 (HTTP)
- **PostgreSQL**: localhost:5433
- **Redis**: localhost:6380

## Project Structure

```
edushield-backend/
├── src/
│   ├── Api/
│   │   └── EduShield.Api/          # Web API Project
│   └── Core/
│       └── EduShield.Core/         # Core Domain Logic
├── tests/
│   └── Api/
│       └── EduShield.Api.Tests/    # API Tests
├── docker-compose.yml              # Infrastructure Services
├── run.sh                          # Quick Start Script
└── README.md                       # This File
```

## Development

### Adding Migrations

```bash
cd src/Api/EduShield.Api
dotnet ef migrations add YourMigrationName
dotnet ef database update
```

### Running Tests

```bash
dotnet test
```

## Configuration

Configuration is managed through:
- `appsettings.json` - Base configuration
- `appsettings.Development.json` - Development overrides
- Environment variables

## Docker Services

The application uses Docker Compose to run:
- **PostgreSQL 15**: Database server on port 5433
- **Redis 7**: Caching server on port 6380

Both services include health checks and persistent volumes.
