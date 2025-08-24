# StudentPerformance Module - EduShield Backend

## Overview

The StudentPerformance module is a comprehensive academic performance tracking system that securely stores and manages student exam results with role-based access control and AES encryption for sensitive data.

## 🏗️ Architecture

### Core Components

1. **Entity Model** (`StudentPerformance`)
   - Tracks academic performance with encrypted scores
   - Maintains relationships with Student entities
   - Includes audit trails and computed properties

2. **Data Layer**
   - Repository pattern with Entity Framework Core
   - PostgreSQL database with encrypted score storage
   - Comprehensive querying capabilities

3. **Service Layer**
   - Business logic and validation
   - Score encryption/decryption handling
   - Data transformation between entities and DTOs

4. **API Layer**
   - RESTful endpoints with role-based access control
   - Comprehensive error handling and logging
   - Swagger documentation

5. **Security Layer**
   - AES encryption for sensitive score data
   - Policy-based authorization
   - Role-based access control

## 🔐 Security Features

### Score Encryption
- **AES-256 encryption** for all score data
- Encryption key derived from JWT secret
- Automatic encryption before storage, decryption before retrieval
- Secure IV generation from key hash

### Access Control Matrix

| Role | Create | Read | Update | Delete | Scope |
|------|--------|------|--------|--------|-------|
| **Admin** | ✅ | ✅ | ✅ | ✅ | All records |
| **DevAuth** | ✅ | ✅ | ✅ | ✅ | All records |
| **Faculty** | ✅ | ✅ | ✅ | ❌ | Assigned students only |
| **Student** | ❌ | ✅ | ❌ | ❌ | Own records only |
| **Parent** | ❌ | ✅ | ❌ | ❌ | Children's records only |

## 📊 Data Model

### StudentPerformance Entity

```csharp
public class StudentPerformance : AuditableEntity
{
    public Guid Id { get; set; }
    public Guid StudentId { get; set; }
    public Student Student { get; set; }
    public string Subject { get; set; }
    public ExamType ExamType { get; set; }
    public DateTime ExamDate { get; set; }
    public string EncryptedScore { get; set; }  // AES encrypted
    public decimal? MaxScore { get; set; }
    public string? ExamTitle { get; set; }
    public string? Comments { get; set; }
    
    // Computed properties
    public decimal Score { get; set; }  // Decrypted value
    public decimal? Percentage { get; }
    public string Grade { get; }
}
```

### Exam Types

- **UnitTest** - Quizzes and small assessments
- **MidTerm** - Mid-semester examinations
- **Final** - End-of-semester examinations
- **Assignment** - Projects and homework
- **Laboratory** - Lab work and practicals
- **Presentation** - Oral presentations
- **ContinuousAssessment** - Ongoing evaluation
- **Other** - Miscellaneous assessments

## 🚀 API Endpoints

### Base Route: `/api/v1/student-performance`

| Method | Endpoint | Description | Access Control |
|--------|----------|-------------|----------------|
| `GET` | `/` | Get all performance records (filtered by role) | All authenticated users |
| `GET` | `/{id}` | Get specific performance record | Role-restricted |
| `POST` | `/` | Create new performance record | Admin, DevAuth, Faculty |
| `PUT` | `/{id}` | Update existing record | Admin, DevAuth, Faculty |
| `DELETE` | `/{id}` | Delete performance record | Admin, DevAuth only |
| `GET` | `/statistics/{studentId}` | Get student statistics | Role-restricted |
| `GET` | `/subject/{subject}` | Get by subject | Role-restricted |
| `GET` | `/exam-type/{examType}` | Get by exam type | Role-restricted |

### Query Parameters

- `subject` - Filter by subject name
- `examType` - Filter by exam type
- `startDate` - Filter by start date (ISO format)
- `endDate` - Filter by end date (ISO format)

## 🔧 Configuration

### Required Services

```csharp
// In Program.cs
builder.Services.AddScoped<IStudentPerformanceRepository, StudentPerformanceRepository>();
builder.Services.AddScoped<IStudentPerformanceService, StudentPerformanceService>();
builder.Services.AddScoped<IEncryptionService, EncryptionService>();
builder.Services.AddScoped<IAuthorizationHandler, StudentPerformanceAuthorizationHandler>();
```

### Database Configuration

The module automatically creates the necessary database tables and relationships:

```sql
CREATE TABLE "StudentPerformances" (
    "Id" uuid PRIMARY KEY,
    "StudentId" uuid NOT NULL,
    "Subject" varchar(100) NOT NULL,
    "ExamType" integer NOT NULL,
    "ExamDate" timestamp NOT NULL,
    "EncryptedScore" text NOT NULL,
    "MaxScore" decimal(18,2),
    "ExamTitle" varchar(200),
    "Comments" varchar(500),
    "CreatedAt" timestamp NOT NULL,
    "UpdatedAt" timestamp NOT NULL,
    FOREIGN KEY ("StudentId") REFERENCES "Students"("Id") ON DELETE CASCADE
);

CREATE UNIQUE INDEX "IX_StudentPerformances_StudentId_Subject_ExamType_ExamDate" 
ON "StudentPerformances" ("StudentId", "Subject", "ExamType", "ExamDate");
```

## 🧪 Testing

### Test Coverage

The module includes comprehensive tests covering:

- **Authorization Tests**
  - Faculty cannot access unassigned students
  - Students cannot view others' data
  - Admin has full access

- **Functionality Tests**
  - CRUD operations
  - Data validation
  - Error handling

- **Security Tests**
  - Encryption/decryption
  - Access control enforcement

### Running Tests

```bash
cd edushield-backend/tests/Api/EduShield.Api.Tests
dotnet test --filter "StudentPerformance"
```

## 📈 Usage Examples

### Creating a Performance Record

```csharp
var request = new CreateStudentPerformanceRequest
{
    StudentId = Guid.Parse("student-guid"),
    Subject = "Mathematics",
    ExamType = ExamType.MidTerm,
    ExamDate = DateTime.Today.AddDays(-1),
    Score = 85.5m,
    MaxScore = 100m,
    ExamTitle = "Mid-Term Mathematics Exam",
    Comments = "Good understanding of algebra concepts"
};

var performance = await _performanceService.CreateAsync(request);
```

### Retrieving Student Performance

```csharp
// Get all performance records for a student
var performances = await _performanceService.GetByStudentIdAsync(studentId);

// Get performance by subject
var mathScores = await _performanceService.GetBySubjectAsync("Mathematics");

// Get performance statistics
var stats = await _performanceService.GetStudentStatisticsAsync(studentId, "Mathematics");
```

## 🔒 Security Considerations

### Encryption Details

- **Algorithm**: AES-256-CBC
- **Key Source**: JWT secret key (first 32 bytes)
- **IV Generation**: SHA-256 hash of key (first 16 bytes)
- **Storage**: Base64 encoded encrypted data

### Access Control

- **JWT Token Required**: All endpoints require valid authentication
- **Role Validation**: Server-side role verification
- **Data Isolation**: Users can only access authorized data
- **Audit Logging**: All operations are logged for security

## 🚨 Error Handling

### Common Error Responses

| Status Code | Error Type | Description |
|-------------|------------|-------------|
| `400` | Bad Request | Validation errors, invalid data |
| `401` | Unauthorized | Missing or invalid JWT token |
| `403` | Forbidden | Insufficient permissions |
| `404` | Not Found | Performance record not found |
| `500` | Internal Server Error | Unexpected system errors |

### Error Response Format

```json
{
  "error": "Error message description",
  "details": "Additional error context if available"
}
```

## 🔄 Migration

### Database Migration

```bash
# Create migration
cd edushield-backend/src/Core/EduShield.Core
dotnet ef migrations add AddStudentPerformance

# Apply migration
dotnet ef database update
```

### Rollback

```bash
# Remove migration
dotnet ef migrations remove

# Or rollback to specific migration
dotnet ef database update PreviousMigrationName
```

## 📚 Dependencies

### Core Dependencies

- **Entity Framework Core** - Data access and ORM
- **PostgreSQL** - Database provider
- **System.Security.Cryptography** - AES encryption
- **Microsoft.AspNetCore.Authorization** - Policy-based authorization

### Internal Dependencies

- **EduShield.Core.Entities** - Domain models
- **EduShield.Core.Interfaces** - Repository and service contracts
- **EduShield.Core.Security** - Encryption services
- **EduShield.Core.Enums** - Domain enums

## 🚀 Future Enhancements

### Planned Features

1. **Performance Analytics**
   - Trend analysis over time
   - Comparative performance metrics
   - Predictive grade modeling

2. **Advanced Filtering**
   - Date range queries
   - Subject combinations
   - Performance thresholds

3. **Bulk Operations**
   - Batch import of exam results
   - Mass updates for grade corrections
   - Bulk deletion with validation

4. **Performance Alerts**
   - Low performance notifications
   - Improvement tracking
   - Parent communication system

### Integration Points

- **Notification System** - Performance alerts and updates
- **Reporting Engine** - Academic performance reports
- **Analytics Dashboard** - Performance visualization
- **Parent Portal** - Secure access to children's performance

## 📞 Support

For questions or issues related to the StudentPerformance module:

1. Check the test suite for usage examples
2. Review the API documentation in Swagger
3. Examine the authorization policies
4. Check the application logs for detailed error information

## 📄 License

This module is part of the EduShield Backend project and follows the same licensing terms.
