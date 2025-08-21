#!/bin/bash

echo "🚀 Starting EduShield Backend..."

# Check if Docker is running
if ! docker info > /dev/null 2>&1; then
    echo "❌ Docker is not running. Please start Docker and try again."
    exit 1
fi

# Start PostgreSQL and Redis
echo "📦 Starting PostgreSQL database and Redis cache..."
docker-compose up -d

# Wait for database to be ready
echo "⏳ Waiting for database to be ready..."
sleep 10

# Check database health
if docker-compose ps | grep -q "healthy"; then
    echo "✅ Database is ready!"
else
    echo "⚠️  Database might not be ready yet. Continuing anyway..."
fi

# Navigate to API project
cd src/Api/EduShield.Api

# Restore packages
echo "📦 Restoring NuGet packages..."
dotnet restore

# Build the project
echo "🔨 Building project..."
dotnet build

# Run database migrations (if any exist)
echo "🗄️  Running database migrations..."
dotnet ef database update --no-build || echo "⚠️  No migrations to run or EF tools not available"

# Start the API
echo "🚀 Starting EduShield Backend API..."
echo "📍 API will be available at: https://localhost:3001 (HTTPS) or http://localhost:3000 (HTTP)"
echo "📚 Swagger UI will be available at: https://localhost:3001/swagger"
echo "🔍 Health endpoint available at: http://localhost:3000/api/v1/health"
echo ""
echo "Press Ctrl+C to stop the application"
echo ""

dotnet run
