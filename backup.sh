#!/bin/bash

echo "💾 Creating database backup..."

# Create backup directory if it doesn't exist
mkdir -p backups

# Create timestamped backup
timestamp=$(date +%Y%m%d_%H%M%S)
backup_file="backups/edushield_backend_${timestamp}.sql"

echo "📝 Creating backup: $backup_file"

# Create the backup
docker exec edushield-backend-postgres pg_dump -U postgres -d edushield_backend > "$backup_file"

if [ $? -eq 0 ]; then
    echo "✅ Backup created successfully: $backup_file"
    
    # Compress the backup
    gzip "$backup_file"
    echo "🗜️  Backup compressed: ${backup_file}.gz"
    
    # Show backup size
    backup_size=$(du -h "${backup_file}.gz" | cut -f1)
    echo "📊 Backup size: $backup_size"
    
    # Clean old backups (keep last 10)
    echo "🧹 Cleaning old backups (keeping last 10)..."
    ls -t backups/edushield_backend_*.sql.gz | tail -n +11 | xargs -r rm
    
    echo "🎉 Backup process completed successfully!"
else
    echo "❌ Backup failed!"
    exit 1
fi
