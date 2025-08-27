#!/bin/bash

echo "🚀 Starting EduShield Backend..."

# Check if Docker is running
if ! docker info > /dev/null 2>&1; then
    echo "❌ Docker is not running. Please start Docker and try again."
    exit 1
fi

# Create necessary directories if they don't exist
echo "📁 Creating necessary directories..."
mkdir -p backups init-scripts

# Stop existing containers gracefully
echo "🛑 Stopping existing containers..."
docker-compose down --remove-orphans

# Start PostgreSQL and Redis with improved persistence
echo "📦 Starting PostgreSQL database and Redis cache with improved persistence..."
docker-compose up -d

# Wait for database to be ready with better health checking
echo "⏳ Waiting for database to be ready..."
max_attempts=30
attempt=1

while [ $attempt -le $max_attempts ]; do
    if docker-compose ps | grep -q "healthy"; then
        echo "✅ Database is ready!"
        break
    fi
    
    echo "⏳ Attempt $attempt/$max_attempts - Waiting for database health check..."
    sleep 10
    attempt=$((attempt + 1))
    
    if [ $attempt -gt $max_attempts ]; then
        echo "⚠️  Database health check timeout. Continuing anyway..."
    fi
done

# Check if database exists, create if it doesn't
echo "🗄️  Checking database existence..."
if ! docker exec edushield-backend-postgres psql -U postgres -lqt | cut -d \| -f 1 | grep -qw edushield_backend; then
    echo "📝 Creating database edushield_backend..."
    docker exec edushield-backend-postgres psql -U postgres -c "CREATE DATABASE edushield_backend;"
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

# Go back to root directory
cd ../../..

# Create a backup before starting
echo "💾 Creating initial backup..."
docker-compose run --rm backup || echo "⚠️  Backup service not available, continuing..."

# Start the API
echo "🚀 Starting EduShield Backend API..."
echo "📍 API will be available at: https://localhost:5001 (HTTPS) or http://localhost:5000 (HTTP)"
echo "📚 Swagger UI will be available at: https://localhost:5001/swagger"
echo "🔍 Health endpoint available at: http://localhost:5000/api/v1/health"
echo "💾 Database data is persisted in Docker volumes"
echo "🔴 Redis data is persisted in Docker volumes"
echo "📦 Backups are stored in: ./backups"
echo ""
echo "Press Ctrl+C to stop the application"
echo ""

cd src/Api/EduShield.Api
dotnet run
