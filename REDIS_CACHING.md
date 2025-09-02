# Redis Caching Implementation

## Overview

This document describes the Redis caching implementation for the EduShield backend, specifically for Student and Student Performance data.

## Architecture

### Cache Service Interface
- **ICacheService**: Defines the contract for caching operations
- **RedisCacheService**: Implements Redis-based caching using `IDistributedCache`

### Cached Data Types

#### Student Data
- **Cache Key Pattern**: `student_{id}`, `student_email_{email}`, `student_roll_{rollNumber}`
- **Cache Duration**: 15 minutes
- **Cached Methods**: `GetByIdAsync`, `GetByEmailAsync`, `GetByRollNumberAsync`

#### Student Performance Data
- **Cache Key Pattern**: `student_performance_{id}`, `student_performances_{studentId}`
- **Cache Duration**: 10 minutes (shorter due to more frequent updates)
- **Cached Methods**: `GetByIdAsync`, `GetByStudentIdAsync`

## Cache Invalidation Strategy

### Student Cache Invalidation
When a student is created, updated, or deleted:
1. Invalidate all cache keys for that student
2. Cache the new/updated data

### Student Performance Cache Invalidation
When a performance record is created, updated, or deleted:
1. Invalidate the specific performance record cache
2. Invalidate the student's performance list cache
3. Cache the new/updated data

## Configuration

### Redis Connection
```json
{
  "ConnectionStrings": {
    "Redis": "localhost:6380"
  }
}
```

### Service Registration
```csharp
// In Program.cs
builder.Services.AddScoped<ICacheService, RedisCacheService>();
```

## Error Handling

The caching implementation is designed to be **fail-safe**:
- If Redis is unavailable, the application continues to work without caching
- Cache operations are wrapped in try-catch blocks
- Database operations are not affected by cache failures

## Performance Benefits

### Before Caching
- Every request hits the database
- N+1 query problems in some scenarios
- Slower response times for frequently accessed data

### After Caching
- Frequently accessed data served from Redis (sub-millisecond response)
- Reduced database load
- Better scalability for concurrent users

## Monitoring

### Cache Hit/Miss Metrics
You can monitor cache performance by:
1. Checking Redis logs
2. Adding custom metrics to track cache hit rates
3. Monitoring database query reduction

### Redis Commands for Debugging
```bash
# Connect to Redis
redis-cli -p 6380

# List all keys
KEYS *

# Get a specific cached value
GET "student_{id}"

# Check TTL (time to live)
TTL "student_{id}"

# Delete a specific key
DEL "student_{id}"
```

## Testing

Use the provided `test-caching.http` file to test the caching implementation:

1. **Cache Population**: First request should populate cache
2. **Cache Hit**: Second identical request should come from cache
3. **Cache Invalidation**: After updates, cache should be invalidated
4. **Fresh Data**: After invalidation, should fetch fresh data from database

## Best Practices

### Cache Key Naming
- Use consistent naming patterns
- Include entity type and identifier
- Use lowercase for consistency

### Cache Duration
- Student data: 15 minutes (relatively stable)
- Performance data: 10 minutes (more frequently updated)

### Cache Invalidation
- Always invalidate related caches when data changes
- Use helper methods to ensure consistency
- Consider cache warming for frequently accessed data

## Future Enhancements

1. **Cache Warming**: Pre-populate cache with frequently accessed data
2. **Cache Compression**: Compress large objects before caching
3. **Distributed Cache**: Use Redis Cluster for high availability
4. **Cache Analytics**: Add detailed metrics and monitoring
5. **Cache Policies**: Implement different cache policies per data type

## Troubleshooting

### Common Issues

1. **Redis Connection Failed**
   - Check if Redis is running: `docker-compose ps`
   - Verify connection string in appsettings.json
   - Check Redis logs: `docker-compose logs redis`

2. **Cache Not Working**
   - Verify service registration in Program.cs
   - Check if ICacheService is injected correctly
   - Enable debug logging to see cache operations

3. **Stale Data**
   - Check cache invalidation logic
   - Verify cache keys are being invalidated correctly
   - Consider reducing cache duration

### Debug Commands
```bash
# Check Redis status
docker-compose exec redis redis-cli ping

# Monitor Redis commands
docker-compose exec redis redis-cli monitor

# Check Redis memory usage
docker-compose exec redis redis-cli info memory
```
