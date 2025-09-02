# EduShield Backend

A comprehensive .NET 8.0 Web API project with PostgreSQL database, Redis caching, and advanced security features including JWT authentication, AES encryption, and role-based rate limiting.

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

## Features

### 🔐 Security
- **JWT Authentication**: Stateless authentication with refresh tokens
- **Google OAuth Integration**: Social login support
- **AES Encryption**: Field-level encryption for sensitive data
- **Role-Based Authorization**: Admin, Faculty, Parent, Student roles
- **Rate Limiting**: API protection with role-based policies

### ⚡ Performance
- **Redis Caching**: High-performance caching for frequently accessed data
- **Connection Pooling**: Efficient database connections
- **Async/Await**: Non-blocking I/O operations
- **Response Compression**: Optimized response sizes

### 🏗️ Architecture
- **Clean Architecture**: Separation of concerns with dependency injection
- **Repository Pattern**: Data access abstraction
- **Service Layer**: Business logic encapsulation
- **DTO Pattern**: Data transfer object mapping

### 🧪 Testing
- **Unit Tests**: Comprehensive business logic testing
- **Integration Tests**: Full API endpoint coverage
- **Mocking**: Isolated testing with Moq
- **Test Fixtures**: Reusable test infrastructure

## API Endpoints

### Authentication
- **Login**: `POST /api/v1/auth/login`
- **Google OAuth**: `POST /api/v1/auth/google`
- **Refresh Token**: `POST /api/v1/auth/refresh`

### Students
- **List Students**: `GET /api/v1/students`
- **Get Student**: `GET /api/v1/students/{id}`
- **Create Student**: `POST /api/v1/students`
- **Update Student**: `PUT /api/v1/students/{id}`
- **Delete Student**: `DELETE /api/v1/students/{id}`

### Student Performance
- **List Performance**: `GET /api/v1/student-performance`
- **Get Performance**: `GET /api/v1/student-performance/{id}`
- **Create Performance**: `POST /api/v1/student-performance`
- **Update Performance**: `PUT /api/v1/student-performance/{id}`
- **Delete Performance**: `DELETE /api/v1/student-performance/{id}`

### System
- **Health Check**: `GET /api/v1/health`
- **Health Ping**: `GET /api/v1/health/ping`
- **Swagger UI**: `/swagger`
- **Health Checks**: `/health`

## Services

- **API**: http://localhost:8080
- **PostgreSQL**: localhost:5433
- **Redis**: localhost:6380
- **Swagger UI**: http://localhost:8080/swagger

## Project Structure

```
edushield-backend/
├── src/
│   ├── Api/
│   │   └── EduShield.Api/                    # Web API Project
│   │       ├── Controllers/                  # API Controllers
│   │       ├── Middleware/                   # Custom Middleware
│   │       ├── Auth/                         # Authorization Components
│   │       ├── RateLimiting/                 # Rate Limiting Policies
│   │       └── Program.cs                    # Application Entry Point
│   └── Core/
│       └── EduShield.Core/                   # Core Domain Logic
│           ├── Entities/                     # Domain Entities
│           ├── Interfaces/                   # Repository & Service Interfaces
│           ├── Services/                     # Business Logic Services
│           ├── Data/                         # Repository Implementations
│           ├── Dtos/                         # Data Transfer Objects
│           ├── Security/                     # Security Services
│           └── Enums/                        # Domain Enums
├── tests/
│   └── Api/
│       └── EduShield.Api.Tests/              # Test Projects
│           ├── Unit/                         # Unit Tests
│           ├── Integration/                  # Integration Tests
│           └── Fixtures/                     # Test Fixtures
├── docker-compose.yml                        # Infrastructure Services
├── run.sh                                    # Quick Start Script
├── test-caching.http                         # Caching Test Suite
├── test-rate-limiting.http                   # Rate Limiting Test Suite
├── REDIS_CACHING.md                          # Caching Documentation
├── RATE_LIMITING.md                          # Rate Limiting Documentation
├── ARCHITECTURE.md                           # Architecture Documentation
├── API_DOCUMENTATION.md                      # API Documentation
└── README.md                                 # This File
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

### Testing Caching

Use the provided test file to verify Redis caching functionality:

```bash
# Test caching with HTTP requests
# Use the test-caching.http file in your IDE or HTTP client
```

### Testing Rate Limiting

Use the provided test file to verify rate limiting functionality:

```bash
# Test rate limiting with HTTP requests
# Use the test-rate-limiting.http file in your IDE or HTTP client
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
   
   # Redis Configuration
   REDIS_CONNECTION_STRING=localhost:6380
   ```

**⚠️ Security Note:** Never commit the `.env` file to version control. It's already added to `.gitignore`.

## Rate Limiting

The API implements comprehensive rate limiting with role-based policies:

### Rate Limits by User Role
- **Admin**: 500 requests/minute
- **Faculty**: 100 requests/minute
- **Parent**: 50 requests/minute
- **Student**: 30 requests/minute
- **Unauthenticated**: 10 requests/minute

### Special Policies
- **Authentication Endpoints**: 5 requests/minute (prevents brute force)
- **Sensitive Operations**: More restrictive limits for create/update/delete

### Rate Limit Response
When rate limits are exceeded, the API returns:
```json
{
  "error": "Rate limit exceeded",
  "message": "Too many requests. Please try again later.",
  "retryAfter": 60,
  "policy": "StudentPolicy",
  "timestamp": "2024-01-01T00:00:00Z"
}
```

## Caching

The API uses Redis for high-performance caching:

### Cache Policies
- **Student Data**: 15-minute expiration
- **Student Performance**: 10-minute expiration
- **User Data**: 20-minute expiration

### Cache Features
- **Automatic Invalidation**: Cache is cleared when data is updated
- **Graceful Degradation**: System continues to work if cache is unavailable
- **Performance Monitoring**: Cache hit/miss rates are logged

## Security Features

### Authentication
- **JWT Tokens**: Stateless authentication with configurable expiration
- **Google OAuth**: Social login integration
- **Refresh Tokens**: Secure token renewal mechanism

### Authorization
- **Role-Based Access Control**: Different permissions for different user types
- **Resource-Level Authorization**: Users can only access their own data
- **Custom Authorization Handlers**: Flexible permission checking

### Data Protection
- **AES Encryption**: Sensitive fields are encrypted at rest
- **HTTPS**: All communications are encrypted in transit
- **Input Validation**: Comprehensive request validation
- **SQL Injection Prevention**: Parameterized queries via Entity Framework

## Docker Services

The application uses Docker Compose to run:
- **PostgreSQL 15**: Database server on port 5433
- **Redis 7**: Caching server on port 6380

Both services include health checks and persistent volumes.

## Documentation

### Technical Documentation
- **[Architecture Documentation](ARCHITECTURE.md)**: Detailed system architecture and design patterns
- **[API Documentation](API_DOCUMENTATION.md)**: Complete API reference with examples
- **[Caching Documentation](REDIS_CACHING.md)**: Redis caching implementation and usage
- **[Rate Limiting Documentation](RATE_LIMITING.md)**: Rate limiting policies and configuration

### Testing Documentation
- **[Caching Tests](test-caching.http)**: HTTP requests to test caching functionality
- **[Rate Limiting Tests](test-rate-limiting.http)**: HTTP requests to test rate limiting

### Quick Reference
- **API Base URL**: `http://localhost:8080`
- **Swagger UI**: `http://localhost:8080/swagger`
- **Health Check**: `http://localhost:8080/api/v1/health`
- **PostgreSQL**: `localhost:5433`
- **Redis**: `localhost:6380`

## Contributing

1. Fork the repository
2. Create a feature branch
3. Make your changes
4. Add tests for new functionality
5. Ensure all tests pass
6. Submit a pull request

## License

This project is licensed under the MIT License - see the LICENSE file for details.
