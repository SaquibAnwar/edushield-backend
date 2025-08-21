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

### Environment Variables

The application requires the following environment variables to be set:

1. **Copy the example environment file:**
   ```bash
   cp .env.example .env
   ```

2. **Edit `.env` with your actual values:**
   ```bash
   # Google OAuth Configuration
   GOOGLE_CLIENT_ID=your-google-client-id
   GOOGLE_CLIENT_SECRET=your-google-client-secret
   
   # JWT Configuration
   JWT_SECRET_KEY=your-super-secret-jwt-key-with-at-least-32-characters
   JWT_ISSUER=EduShield
   JWT_AUDIENCE=EduShield
   JWT_EXPIRATION_MINUTES=60
   JWT_REFRESH_TOKEN_EXPIRATION_DAYS=7
   
   # Application Configuration
   ENABLE_DEV_AUTH=true
   ```

**⚠️ Security Note:** Never commit the `.env` file to version control. It's already added to `.gitignore`.

## Docker Services

The application uses Docker Compose to run:
- **PostgreSQL 15**: Database server on port 5433
- **Redis 7**: Caching server on port 6380

Both services include health checks and persistent volumes.
