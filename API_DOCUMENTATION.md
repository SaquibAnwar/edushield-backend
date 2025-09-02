# EduShield Backend - API Documentation

## Base URL
```
http://localhost:8080
```

## Authentication

All API endpoints (except authentication endpoints) require a valid JWT token in the Authorization header:

```
Authorization: Bearer <your-jwt-token>
```

## Rate Limiting

The API implements rate limiting with the following policies:

| User Role | Requests per Minute | Special Notes |
|-----------|-------------------|---------------|
| Admin | 500 | Full system access |
| Faculty | 100 | Teaching and academic management |
| Parent | 50 | Child monitoring |
| Student | 30 | Academic information access |
| Unauthenticated | 10 | Limited access |

### Special Rate Limits
- **Authentication Endpoints**: 5 requests/minute (prevents brute force)
- **Sensitive Operations**: More restrictive limits for create/update/delete operations

## Error Responses

### Rate Limit Exceeded (429)
```json
{
  "error": "Rate limit exceeded",
  "message": "Too many requests. Please try again later.",
  "retryAfter": 60,
  "policy": "StudentPolicy",
  "timestamp": "2024-01-01T00:00:00Z"
}
```

### Validation Error (400)
```json
{
  "error": "Validation failed",
  "message": "The request contains invalid data",
  "details": [
    {
      "field": "email",
      "message": "Email is required"
    }
  ],
  "timestamp": "2024-01-01T00:00:00Z",
  "statusCode": 400
}
```

### Unauthorized (401)
```json
{
  "error": "Unauthorized",
  "message": "Invalid or expired token",
  "timestamp": "2024-01-01T00:00:00Z",
  "statusCode": 401
}
```

### Forbidden (403)
```json
{
  "error": "Forbidden",
  "message": "Insufficient permissions",
  "timestamp": "2024-01-01T00:00:00Z",
  "statusCode": 403
}
```

## Authentication Endpoints

### Login
```http
POST /api/v1/auth/login
Content-Type: application/json

{
  "email": "user@example.com",
  "password": "password123"
}
```

**Response (200):**
```json
{
  "token": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",
  "refreshToken": "refresh-token-here",
  "expiresAt": "2024-01-01T01:00:00Z",
  "user": {
    "id": "user-id",
    "email": "user@example.com",
    "firstName": "John",
    "lastName": "Doe",
    "role": "Student"
  }
}
```

### Google OAuth Login
```http
POST /api/v1/auth/google
Content-Type: application/json

{
  "idToken": "google-id-token"
}
```

### Refresh Token
```http
POST /api/v1/auth/refresh
Content-Type: application/json

{
  "refreshToken": "refresh-token-here"
}
```

## Student Endpoints

### List Students
```http
GET /api/v1/students
Authorization: Bearer <token>
```

**Response (200):**
```json
[
  {
    "id": "student-id",
    "firstName": "John",
    "lastName": "Doe",
    "email": "john.doe@example.com",
    "rollNumber": "STU001",
    "gender": "Male",
    "status": "Active",
    "createdAt": "2024-01-01T00:00:00Z",
    "updatedAt": "2024-01-01T00:00:00Z"
  }
]
```

### Get Student by ID
```http
GET /api/v1/students/{id}
Authorization: Bearer <token>
```

### Create Student
```http
POST /api/v1/students
Authorization: Bearer <token>
Content-Type: application/json

{
  "firstName": "Jane",
  "lastName": "Smith",
  "email": "jane.smith@example.com",
  "rollNumber": "STU002",
  "gender": "Female",
  "status": "Active"
}
```

### Update Student
```http
PUT /api/v1/students/{id}
Authorization: Bearer <token>
Content-Type: application/json

{
  "firstName": "Jane",
  "lastName": "Smith-Updated",
  "email": "jane.smith@example.com",
  "rollNumber": "STU002",
  "gender": "Female",
  "status": "Active"
}
```

### Delete Student
```http
DELETE /api/v1/students/{id}
Authorization: Bearer <token>
```

## Student Performance Endpoints

### List Student Performance Records
```http
GET /api/v1/student-performance
Authorization: Bearer <token>
```

**Response (200):**
```json
[
  {
    "id": "performance-id",
    "studentId": "student-id",
    "subject": "Mathematics",
    "examType": "MidTerm",
    "examDate": "2024-01-01T00:00:00Z",
    "score": 85.5,
    "comments": "Good performance",
    "createdAt": "2024-01-01T00:00:00Z",
    "updatedAt": "2024-01-01T00:00:00Z"
  }
]
```

### Get Student Performance by ID
```http
GET /api/v1/student-performance/{id}
Authorization: Bearer <token>
```

### Create Student Performance Record
```http
POST /api/v1/student-performance
Authorization: Bearer <token>
Content-Type: application/json

{
  "studentId": "student-id",
  "subject": "Mathematics",
  "examType": "Final",
  "examDate": "2024-01-01T00:00:00Z",
  "score": 92.0,
  "comments": "Excellent performance"
}
```

### Update Student Performance Record
```http
PUT /api/v1/student-performance/{id}
Authorization: Bearer <token>
Content-Type: application/json

{
  "studentId": "student-id",
  "subject": "Mathematics",
  "examType": "Final",
  "examDate": "2024-01-01T00:00:00Z",
  "score": 95.0,
  "comments": "Outstanding performance"
}
```

### Delete Student Performance Record
```http
DELETE /api/v1/student-performance/{id}
Authorization: Bearer <token>
```

## User Management Endpoints

### List Users
```http
GET /api/v1/user
Authorization: Bearer <token>
```

### Get User by ID
```http
GET /api/v1/user/{id}
Authorization: Bearer <token>
```

### Create User
```http
POST /api/v1/user
Authorization: Bearer <token>
Content-Type: application/json

{
  "email": "newuser@example.com",
  "firstName": "New",
  "lastName": "User",
  "role": "Student"
}
```

### Update User
```http
PUT /api/v1/user/{id}
Authorization: Bearer <token>
Content-Type: application/json

{
  "email": "updated@example.com",
  "firstName": "Updated",
  "lastName": "User",
  "role": "Faculty"
}
```

### Delete User
```http
DELETE /api/v1/user/{id}
Authorization: Bearer <token>
```

## System Endpoints

### Health Check
```http
GET /api/v1/health
```

**Response (200):**
```json
{
  "status": "healthy",
  "timestamp": "2024-01-01T00:00:00Z",
  "service": "EduShield Backend API"
}
```

### Health Ping
```http
GET /api/v1/health/ping
```

**Response (200):**
```json
{
  "message": "pong",
  "timestamp": "2024-01-01T00:00:00Z"
}
```

## Data Models

### Student
```typescript
interface Student {
  id: string;
  firstName: string;
  lastName: string;
  email: string;
  rollNumber: string;
  gender: "Male" | "Female" | "Other";
  status: "Active" | "Inactive" | "Suspended";
  createdAt: string;
  updatedAt: string;
}
```

### Student Performance
```typescript
interface StudentPerformance {
  id: string;
  studentId: string;
  subject: string;
  examType: "Quiz" | "MidTerm" | "Final" | "Assignment";
  examDate: string;
  score: number;
  comments?: string;
  createdAt: string;
  updatedAt: string;
}
```

### User
```typescript
interface User {
  id: string;
  email: string;
  firstName: string;
  lastName: string;
  role: "Admin" | "Faculty" | "Parent" | "Student";
  isActive: boolean;
  createdAt: string;
  updatedAt: string;
}
```

## Caching Behavior

### Cache Keys
- **Student Data**: `student_{id}`, `student_email_{email}`, `student_roll_{rollNumber}`
- **Student Performance**: `student_performance_{id}`, `student_performances_{studentId}`
- **User Data**: `user_{id}`, `user_email_{email}`

### Cache Expiration
- **Student Data**: 15 minutes
- **Student Performance**: 10 minutes
- **User Data**: 20 minutes

### Cache Invalidation
Cache is automatically invalidated when data is updated or deleted.

## Testing

### Test Files
- **Caching Tests**: `test-caching.http`
- **Rate Limiting Tests**: `test-rate-limiting.http`

### Example Test Request
```http
### Test Student Creation with Caching
POST http://localhost:8080/api/v1/students
Authorization: Bearer your-jwt-token
Content-Type: application/json

{
  "firstName": "Test",
  "lastName": "Student",
  "email": "test@example.com",
  "rollNumber": "TEST001",
  "gender": "Male",
  "status": "Active"
}
```

## SDKs and Client Libraries

### JavaScript/TypeScript
```javascript
// Example API client
class EduShieldAPI {
  constructor(baseURL, token) {
    this.baseURL = baseURL;
    this.token = token;
  }

  async getStudents() {
    const response = await fetch(`${this.baseURL}/api/v1/students`, {
      headers: {
        'Authorization': `Bearer ${this.token}`,
        'Content-Type': 'application/json'
      }
    });
    return response.json();
  }
}
```

### C# Client
```csharp
public class EduShieldClient
{
    private readonly HttpClient _httpClient;
    private readonly string _baseUrl;

    public EduShieldClient(HttpClient httpClient, string baseUrl)
    {
        _httpClient = httpClient;
        _baseUrl = baseUrl;
    }

    public async Task<List<Student>> GetStudentsAsync(string token)
    {
        _httpClient.DefaultRequestHeaders.Authorization = 
            new AuthenticationHeaderValue("Bearer", token);
        
        var response = await _httpClient.GetAsync($"{_baseUrl}/api/v1/students");
        response.EnsureSuccessStatusCode();
        
        var json = await response.Content.ReadAsStringAsync();
        return JsonSerializer.Deserialize<List<Student>>(json);
    }
}
```

---

*This API documentation is maintained alongside the codebase and updated with each release.*
