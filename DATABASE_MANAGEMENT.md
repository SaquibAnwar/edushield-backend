# Database Management Guide - EduShield Backend

## Overview

This guide explains how to manage the PostgreSQL database and Redis cache with persistent storage for the EduShield Backend project.

## 🏗️ Architecture

### Persistent Storage Structure
```
edushield-backend/
├── data/
│   ├── postgres/          # PostgreSQL data files (persistent)
│   └── redis/             # Redis data files (persistent)
├── backups/               # Database backup files
├── init-scripts/          # Database initialization scripts
└── docker-compose.yml     # Docker configuration
```

## 🚀 Quick Start

### 1. Start the Application
```bash
./run.sh
```

This script will:
- Create necessary directories
- Start PostgreSQL and Redis containers
- Wait for database health checks
- Create database if it doesn't exist
- Run migrations
- Start the API

### 2. Stop the Application
```bash
docker-compose down
```

### 3. Stop and Remove Data
```bash
docker-compose down -v
```
⚠️ **Warning**: This will remove all data volumes!

## 💾 Database Persistence

### How Persistence Works

1. **Named Volumes**: Docker creates named volumes that persist data
2. **Bind Mounts**: Data is bound to local directories for easy access
3. **Health Checks**: Containers wait for database readiness before starting
4. **Automatic Restart**: Containers restart automatically unless stopped manually

### Data Locations

- **PostgreSQL**: `./data/postgres/`
- **Redis**: `./data/redis/`
- **Backups**: `./backups/`

## 🔄 Backup and Restore

### Automatic Backups

The system creates automatic backups when starting:
```bash
# Backup is created automatically during startup
./run.sh
```

### Manual Backups

Create a manual backup:
```bash
./backup.sh
```

This will:
- Create a timestamped backup file
- Compress the backup
- Clean old backups (keeps last 10)

### Restore from Backup

Restore the database from a backup:
```bash
./restore.sh backups/edushield_backend_20241201_120000.sql.gz
```

⚠️ **Warning**: This will overwrite the current database!

## 🛠️ Database Management

### Check Database Status
```bash
# Check container health
docker-compose ps

# Check database logs
docker-compose logs postgres

# Check Redis logs
docker-compose logs redis
```

### Access Database
```bash
# Connect to PostgreSQL
docker exec -it edushield-backend-postgres psql -U postgres -d edushield_backend

# Connect to Redis
docker exec -it edushield-backend-redis redis-cli
```

### Database Operations
```sql
-- List all tables
\dt

-- Check table structure
\d+ table_name

-- Check database size
SELECT pg_size_pretty(pg_database_size('edushield_backend'));

-- Check table sizes
SELECT 
    schemaname,
    tablename,
    pg_size_pretty(pg_total_relation_size(schemaname||'.'||tablename)) as size
FROM pg_tables 
WHERE schemaname = 'public'
ORDER BY pg_total_relation_size(schemaname||'.'||tablename) DESC;
```

## 🔧 Troubleshooting

### Common Issues

#### 1. Database Connection Failed
```bash
# Check if containers are running
docker-compose ps

# Check container logs
docker-compose logs postgres

# Restart containers
docker-compose restart
```

#### 2. Data Loss After Restart
```bash
# Check volume mounts
docker volume ls

# Check data directory permissions
ls -la data/

# Recreate containers with existing volumes
docker-compose down
docker-compose up -d
```

#### 3. Migration Errors
```bash
# Check migration status
cd src/Api/EduShield.Api
dotnet ef migrations list

# Reset database (⚠️ WARNING: Data loss!)
dotnet ef database drop
dotnet ef database update
```

### Reset Everything

If you need to start completely fresh:
```bash
# Stop and remove everything
docker-compose down -v

# Remove data directories
rm -rf data/ backups/

# Start fresh
./run.sh
```

## 📊 Monitoring

### Database Performance
```sql
-- Check active connections
SELECT count(*) FROM pg_stat_activity WHERE state = 'active';

-- Check slow queries
SELECT query, mean_time, calls 
FROM pg_stat_statements 
ORDER BY mean_time DESC 
LIMIT 10;

-- Check table statistics
SELECT schemaname, tablename, n_tup_ins, n_tup_upd, n_tup_del
FROM pg_stat_user_tables;
```

### Redis Performance
```bash
# Connect to Redis and check info
docker exec -it edushield-backend-redis redis-cli info

# Check memory usage
docker exec -it edushield-backend-redis redis-cli info memory

# Check connected clients
docker exec -it edushield-backend-redis redis-cli client list
```

## 🔒 Security Considerations

### Database Security
- Database is only accessible from localhost
- Default credentials are for development only
- Production should use strong passwords and SSL

### Backup Security
- Backups contain sensitive data
- Store backups securely
- Consider encrypting backup files

## 📚 Best Practices

1. **Regular Backups**: Create backups before major changes
2. **Test Restores**: Periodically test backup restoration
3. **Monitor Space**: Keep track of data directory sizes
4. **Version Control**: Keep database schema in version control
5. **Documentation**: Document any custom database changes

## 🚨 Emergency Procedures

### Complete System Failure
```bash
# 1. Stop all containers
docker-compose down

# 2. Check Docker status
docker info

# 3. Restart Docker if needed
# 4. Start fresh
./run.sh
```

### Data Corruption
```bash
# 1. Stop the application
docker-compose down

# 2. Restore from latest backup
./restore.sh backups/latest_backup.sql.gz

# 3. Restart application
./run.sh
```

## 📞 Support

For database-related issues:
1. Check the logs: `docker-compose logs postgres`
2. Verify volume mounts: `docker volume ls`
3. Check data directory permissions
4. Review this documentation
5. Check the main README.md for general troubleshooting

## 📄 License

This guide is part of the EduShield Backend project and follows the same licensing terms.
