#!/bin/bash

if [ $# -eq 0 ]; then
    echo "❌ Please provide a backup file to restore from"
    echo "Usage: ./restore.sh <backup_file>"
    echo "Example: ./restore.sh backups/edushield_backend_20241201_120000.sql.gz"
    exit 1
fi

backup_file="$1"

if [ ! -f "$backup_file" ]; then
    echo "❌ Backup file not found: $backup_file"
    exit 1
fi

echo "🔄 Restoring database from: $backup_file"

# Check if database exists
if docker exec edushield-backend-postgres psql -U postgres -lqt | cut -d \| -f 1 | grep -qw edushield_backend; then
    echo "🗄️  Dropping existing database..."
    docker exec edushield-backend-postgres psql -U postgres -c "DROP DATABASE edushield_backend;"
fi

echo "📝 Creating new database..."
docker exec edushield-backend-postgres psql -U postgres -c "CREATE DATABASE edushield_backend;"

echo "🔄 Restoring data..."
if [[ "$backup_file" == *.gz ]]; then
    # Compressed backup
    gunzip -c "$backup_file" | docker exec -i edushield-backend-postgres psql -U postgres -d edushield_backend
else
    # Uncompressed backup
    docker exec -i edushield-backend-postgres psql -U postgres -d edushield_backend < "$backup_file"
fi

if [ $? -eq 0 ]; then
    echo "✅ Database restored successfully!"
    echo "🔄 Running migrations to ensure schema is up to date..."
    cd src/Api/EduShield.Api
    dotnet ef database update --no-build
    echo "🎉 Restore process completed successfully!"
else
    echo "❌ Database restore failed!"
    exit 1
fi
