using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using EduShield.Core.Enums;

namespace EduShield.Api.Controllers;

/// <summary>
/// Test controller providing endpoints to demonstrate and test authentication, authorization, and role-based access control
/// </summary>
[ApiController]
[Route("api/v1/test")]
[Produces("application/json")]
[ProducesResponseType(typeof(ProblemDetails), 400)]
[ProducesResponseType(typeof(ProblemDetails), 401)]
[ProducesResponseType(typeof(ProblemDetails), 403)]
[ProducesResponseType(typeof(ProblemDetails), 500)]
public class TestController : ControllerBase
{
    /// <summary>
    /// Public endpoint that requires no authentication
    /// </summary>
    /// <remarks>
    /// This endpoint is accessible to anyone without authentication.
    /// It's useful for testing that the API is running and accessible.
    /// 
    /// **Use Cases:**
    /// - Health checks
    /// - Public information
    /// - Testing API connectivity
    /// 
    /// **Sample Response:**
    /// ```json
    /// {
    ///   "message": "This is a public endpoint - no authentication required"
    /// }
    /// ```
    /// </remarks>
    /// <returns>Public message indicating no authentication is required</returns>
    /// <response code="200">Successfully accessed public endpoint.</response>
    [HttpGet("public")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(object), 200)]
    public IActionResult PublicEndpoint()
    {
        return Ok(new { message = "This is a public endpoint - no authentication required" });
    }

    /// <summary>
    /// Endpoint that requires any valid authentication
    /// </summary>
    /// <remarks>
    /// This endpoint requires a valid JWT token but doesn't check for specific roles.
    /// Any authenticated user (Admin, Student, Faculty, Parent, or DevAuth) can access it.
    /// 
    /// **Use Cases:**
    /// - User profile information
    /// - General authenticated features
    /// - Testing JWT token validation
    /// 
    /// **Required Headers:**
    /// ```
    /// Authorization: Bearer YOUR_JWT_TOKEN
    /// ```
    /// 
    /// **Sample Response:**
    /// ```json
    /// {
    ///   "message": "This endpoint requires authentication",
    ///   "user": {
    ///     "email": "iamsaquibanwar@gmail.com",
    ///     "role": "Admin"
    ///   }
    /// }
    /// ```
    /// </remarks>
    /// <returns>Message confirming authentication and user information</returns>
    /// <response code="200">Successfully accessed authenticated endpoint.</response>
    /// <response code="401">Unauthorized. Valid JWT token required.</response>
    [HttpGet("authenticated")]
    [Authorize(Policy = "AuthenticatedUser")]
    [ProducesResponseType(typeof(object), 200)]
    public IActionResult AuthenticatedEndpoint()
    {
        var userEmail = User.FindFirst(ClaimTypes.Email)?.Value;
        var userRole = User.FindFirst(ClaimTypes.Role)?.Value;
        
        return Ok(new 
        { 
            message = "This endpoint requires authentication",
            user = new { email = userEmail, role = userRole }
        });
    }

    /// <summary>
    /// Endpoint restricted to Admin users only
    /// </summary>
    /// <remarks>
    /// This endpoint can only be accessed by users with the Admin role.
    /// Other roles (Student, Faculty, Parent, DevAuth) will receive a 403 Forbidden response.
    /// 
    /// **Use Cases:**
    /// - System administration
    /// - User management
    /// - System configuration
    /// - Testing role-based access control
    /// 
    /// **Required Headers:**
    /// ```
    /// Authorization: Bearer ADMIN_JWT_TOKEN
    /// ```
    /// 
    /// **Sample Response:**
    /// ```json
    /// {
    ///   "message": "This endpoint is for admins only",
    ///   "user": "iamsaquibanwar@gmail.com"
    /// }
    /// ```
    /// 
    /// **Access Control:**
    /// - ✅ Admin users
    /// - ❌ Student users
    /// - ❌ Faculty users
    /// - ❌ Parent users
    /// - ❌ DevAuth users
    /// </remarks>
    /// <returns>Message confirming admin access and user information</returns>
    /// <response code="200">Successfully accessed admin-only endpoint.</response>
    /// <response code="401">Unauthorized. Valid JWT token required.</response>
    /// <response code="403">Forbidden. Admin role required.</response>
    [HttpGet("admin-only")]
    [Authorize(Policy = "AdminOnly")]
    [ProducesResponseType(typeof(object), 200)]
    public IActionResult AdminOnlyEndpoint()
    {
        var userEmail = User.FindFirst(ClaimTypes.Email)?.Value;
        
        return Ok(new 
        { 
            message = "This endpoint is for admins only",
            user = userEmail
        });
    }

    /// <summary>
    /// Endpoint restricted to Student users only
    /// </summary>
    /// <remarks>
    /// This endpoint can only be accessed by users with the Student role.
    /// Other roles (Admin, Faculty, Parent, DevAuth) will receive a 403 Forbidden response.
    /// 
    /// **Use Cases:**
    /// - Student-specific features
    /// - Course enrollment
    /// - Grade viewing
    /// - Testing role-based access control
    /// 
    /// **Required Headers:**
    /// ```
    /// Authorization: Bearer STUDENT_JWT_TOKEN
    /// ```
    /// 
    /// **Sample Response:**
    /// ```json
    /// {
    ///   "message": "This endpoint is for students only",
    ///   "user": "saquibanwar01@gmail.com"
    /// }
    /// ```
    /// 
    /// **Access Control:**
    /// - ❌ Admin users
    /// - ✅ Student users
    /// - ❌ Faculty users
    /// - ❌ Parent users
    /// - ❌ DevAuth users
    /// </remarks>
    /// <returns>Message confirming student access and user information</returns>
    /// <response code="200">Successfully accessed student-only endpoint.</response>
    /// <response code="401">Unauthorized. Valid JWT token required.</response>
    /// <response code="403">Forbidden. Student role required.</response>
    [HttpGet("student-only")]
    [Authorize(Policy = "StudentOnly")]
    [ProducesResponseType(typeof(object), 200)]
    public IActionResult StudentOnlyEndpoint()
    {
        var userEmail = User.FindFirst(ClaimTypes.Email)?.Value;
        
        return Ok(new 
        { 
            message = "This endpoint is for students only",
            user = userEmail
        });
    }

    /// <summary>
    /// Endpoint restricted to Faculty users only
    /// </summary>
    /// <remarks>
    /// This endpoint can only be accessed by users with the Faculty role.
    /// Other roles (Admin, Student, Parent, DevAuth) will receive a 403 Forbidden response.
    /// 
    /// **Use Cases:**
    /// - Faculty-specific features
    /// - Course management
    /// - Grade management
    /// - Testing role-based access control
    /// 
    /// **Required Headers:**
    /// ```
    /// Authorization: Bearer FACULTY_JWT_TOKEN
    /// ```
    /// 
    /// **Sample Response:**
    /// ```json
    /// {
    ///   "message": "This endpoint is for faculty only",
    ///   "user": "saquibedu@gmail.com"
    /// }
    /// ```
    /// 
    /// **Access Control:**
    /// - ❌ Admin users
    /// - ❌ Student users
    /// - ✅ Faculty users
    /// - ❌ Parent users
    /// - ❌ DevAuth users
    /// </remarks>
    /// <returns>Message confirming faculty access and user information</returns>
    /// <response code="200">Successfully accessed faculty-only endpoint.</response>
    /// <response code="401">Unauthorized. Valid JWT token required.</response>
    /// <response code="403">Forbidden. Faculty role required.</response>
    [HttpGet("faculty-only")]
    [Authorize(Policy = "FacultyOnly")]
    [ProducesResponseType(typeof(object), 200)]
    public IActionResult FacultyOnlyEndpoint()
    {
        var userEmail = User.FindFirst(ClaimTypes.Email)?.Value;
        
        return Ok(new 
        { 
            message = "This endpoint is for faculty only",
            user = userEmail
        });
    }

    /// <summary>
    /// Endpoint restricted to Parent users only
    /// </summary>
    /// <remarks>
    /// This endpoint can only be accessed by users with the Parent role.
    /// Other roles (Admin, Student, Faculty, DevAuth) will receive a 403 Forbidden response.
    /// 
    /// **Use Cases:**
    /// - Parent-specific features
    /// - Child progress monitoring
    /// - Fee management
    /// - Testing role-based access control
    /// 
    /// **Required Headers:**
    /// ```
    /// Authorization: Bearer PARENT_JWT_TOKEN
    /// ```
    /// 
    /// **Sample Response:**
    /// ```json
    /// {
    ///   "message": "This endpoint is for parents only",
    ///   "user": "kirakryto9ite@gmail.com"
    /// }
    /// ```
    /// 
    /// **Access Control:**
    /// - ❌ Admin users
    /// - ❌ Student users
    /// - ❌ Faculty users
    /// - ✅ Parent users
    /// - ❌ DevAuth users
    /// </remarks>
    /// <returns>Message confirming parent access and user information</returns>
    /// <response code="200">Successfully accessed parent-only endpoint.</response>
    /// <response code="401">Unauthorized. Valid JWT token required.</response>
    /// <response code="403">Forbidden. Parent role required.</response>
    [HttpGet("parent-only")]
    [Authorize(Policy = "ParentOnly")]
    [ProducesResponseType(typeof(object), 200)]
    public IActionResult ParentOnlyEndpoint()
    {
        var userEmail = User.FindFirst(ClaimTypes.Email)?.Value;
        
        return Ok(new 
        { 
            message = "This endpoint is for parents only",
            user = userEmail
        });
    }

    /// <summary>
    /// Endpoint restricted to DevAuth users only
    /// </summary>
    /// <remarks>
    /// This endpoint can only be accessed by users with the DevAuth role.
    /// Other roles (Admin, Student, Faculty, Parent) will receive a 403 Forbidden response.
    /// 
    /// **Use Cases:**
    /// - Development and testing
    /// - Debugging features
    /// - System diagnostics
    /// - Testing role-based access control
    /// 
    /// **Required Headers:**
    /// ```
    /// Authorization: Bearer DEVAUTH_JWT_TOKEN
    /// ```
    /// 
    /// **Sample Response:**
    /// ```json
    /// {
    ///   "message": "This endpoint is for dev auth users only",
    ///   "user": "techtonicwave.business@gmail.com"
    /// }
    /// ```
    /// 
    /// **Access Control:**
    /// - ❌ Admin users
    /// - ❌ Student users
    /// - ❌ Faculty users
    /// - ❌ Parent users
    /// - ✅ DevAuth users
    /// </remarks>
    /// <returns>Message confirming dev auth access and user information</returns>
    /// <response code="200">Successfully accessed dev auth only endpoint.</response>
    /// <response code="401">Unauthorized. Valid JWT token required.</response>
    /// <response code="403">Forbidden. DevAuth role required.</response>
    [HttpGet("dev-auth-only")]
    [Authorize(Policy = "DevAuthOnly")]
    [ProducesResponseType(typeof(object), 200)]
    public IActionResult DevAuthOnlyEndpoint()
    {
        var userEmail = User.FindFirst(ClaimTypes.Email)?.Value;
        
        return Ok(new 
        { 
            message = "This endpoint is for dev auth users only",
            user = userEmail
        });
    }

    /// <summary>
    /// Endpoint accessible by Admin or Faculty users
    /// </summary>
    /// <remarks>
    /// This endpoint can be accessed by users with either Admin or Faculty roles.
    /// Other roles (Student, Parent, DevAuth) will receive a 403 Forbidden response.
    /// 
    /// **Use Cases:**
    /// - Academic management
    /// - Course oversight
    /// - Administrative tasks
    /// - Testing combined role access control
    /// 
    /// **Required Headers:**
    /// ```
    /// Authorization: Bearer ADMIN_OR_FACULTY_JWT_TOKEN
    /// ```
    /// 
    /// **Sample Response:**
    /// ```json
    /// {
    ///   "message": "This endpoint is for admins or faculty",
    ///   "user": "iamsaquibanwar@gmail.com",
    ///   "role": "Admin"
    /// }
    /// ```
    /// 
    /// **Access Control:**
    /// - ✅ Admin users
    /// - ❌ Student users
    /// - ✅ Faculty users
    /// - ❌ Parent users
    /// - ❌ DevAuth users
    /// </remarks>
    /// <returns>Message confirming admin or faculty access and user information</returns>
    /// <response code="200">Successfully accessed admin or faculty endpoint.</response>
    /// <response code="401">Unauthorized. Valid JWT token required.</response>
    /// <response code="403">Forbidden. Admin or Faculty role required.</response>
    [HttpGet("admin-or-faculty")]
    [Authorize(Policy = "AdminOrFaculty")]
    [ProducesResponseType(typeof(object), 200)]
    public IActionResult AdminOrFacultyEndpoint()
    {
        var userEmail = User.FindFirst(ClaimTypes.Email)?.Value;
        var userRole = User.FindFirst(ClaimTypes.Role)?.Value;
        
        return Ok(new 
        { 
            message = "This endpoint is for admins or faculty",
            user = userEmail,
            role = userRole
        });
    }

    /// <summary>
    /// Endpoint accessible by Admin or Student users
    /// </summary>
    /// <remarks>
    /// This endpoint can be accessed by users with either Admin or Student roles.
    /// Other roles (Faculty, Parent, DevAuth) will receive a 403 Forbidden response.
    /// 
    /// **Use Cases:**
    /// - Student information access
    /// - Academic records
    /// - Student support
    /// - Testing combined role access control
    /// 
    /// **Required Headers:**
    /// ```
    /// Authorization: Bearer ADMIN_OR_STUDENT_JWT_TOKEN
    /// ```
    /// 
    /// **Sample Response:**
    /// ```json
    /// {
    ///   "message": "This endpoint is for admins or students",
    ///   "user": "iamsaquibanwar@gmail.com",
    ///   "role": "Admin"
    /// }
    /// ```
    /// 
    /// **Access Control:**
    /// - ✅ Admin users
    /// - ✅ Student users
    /// - ❌ Faculty users
    /// - ❌ Parent users
    /// - ❌ DevAuth users
    /// </remarks>
    /// <returns>Message confirming admin or student access and user information</returns>
    /// <response code="200">Successfully accessed admin or student endpoint.</response>
    /// <response code="401">Unauthorized. Valid JWT token required.</response>
    /// <response code="403">Forbidden. Admin or Student role required.</response>
    [HttpGet("admin-or-student")]
    [Authorize(Policy = "AdminOrStudent")]
    [ProducesResponseType(typeof(object), 200)]
    public IActionResult AdminOrStudentEndpoint()
    {
        var userEmail = User.FindFirst(ClaimTypes.Email)?.Value;
        var userRole = User.FindFirst(ClaimTypes.Role)?.Value;
        
        return Ok(new 
        { 
            message = "This endpoint is for admins or students",
            user = userEmail,
            role = userRole
        });
    }

    /// <summary>
    /// Endpoint accessible by Admin or Parent users
    /// </summary>
    /// <remarks>
    /// This endpoint can be accessed by users with either Admin or Parent roles.
    /// Other roles (Student, Faculty, DevAuth) will receive a 403 Forbidden response.
    /// 
    /// **Use Cases:**
    /// - Parent communication
    /// - Family information
    /// - Parental oversight
    /// - Testing combined role access control
    /// 
    /// **Required Headers:**
    /// ```
    /// Authorization: Bearer ADMIN_OR_PARENT_JWT_TOKEN
    /// ```
    /// 
    /// **Sample Response:**
    /// ```json
    /// {
    ///   "message": "This endpoint is for admins or parents",
    ///   "user": "iamsaquibanwar@gmail.com",
    ///   "role": "Admin"
    /// }
    /// ```
    /// 
    /// **Access Control:**
    /// - ✅ Admin users
    /// - ❌ Student users
    /// - ❌ Faculty users
    /// - ✅ Parent users
    /// - ❌ DevAuth users
    /// </remarks>
    /// <returns>Message confirming admin or parent access and user information</returns>
    /// <response code="200">Successfully accessed admin or parent endpoint.</response>
    /// <response code="401">Unauthorized. Valid JWT token required.</response>
    /// <response code="403">Forbidden. Admin or Parent role required.</response>
    [HttpGet("admin-or-parent")]
    [Authorize(Policy = "AdminOrParent")]
    [ProducesResponseType(typeof(object), 200)]
    public IActionResult AdminOrParentEndpoint()
    {
        var userEmail = User.FindFirst(ClaimTypes.Email)?.Value;
        var userRole = User.FindFirst(ClaimTypes.Role)?.Value;
        
        return Ok(new 
        { 
            message = "This endpoint is for admins or parents",
            user = userEmail,
            role = userRole
        });
    }

    /// <summary>
    /// Endpoint that displays current user information from JWT claims
    /// </summary>
    /// <remarks>
    /// This endpoint extracts and displays all the claims from the current user's JWT token.
    /// It's useful for debugging authentication and understanding what information is available in the token.
    /// 
    /// **Use Cases:**
    /// - Debugging authentication
    /// - Understanding JWT token contents
    /// - Testing claim extraction
    /// - Development and testing
    /// 
    /// **Required Headers:**
    /// ```
    /// Authorization: Bearer ANY_VALID_JWT_TOKEN
    /// ```
    /// 
    /// **Sample Response:**
    /// ```json
    /// {
    ///   "message": "Current user information",
    ///   "claims": [
    ///     {
    ///       "type": "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier",
    ///       "value": "443abd4f-9e56-4adc-9eb7-7a0e2522dd2b"
    ///     },
    ///     {
    ///       "type": "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/emailaddress",
    ///       "value": "iamsaquibanwar@gmail.com"
    ///     },
    ///     {
    ///       "type": "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/name",
    ///       "value": "Saquib Admin"
    ///     },
    ///     {
    ///       "type": "http://schemas.microsoft.com/ws/2008/06/identity/claims/role",
    ///       "value": "Admin"
    ///     }
    ///   ]
    /// }
    /// ```
    /// 
    /// **Available Claims:**
    /// - `nameidentifier`: User's unique ID
    /// - `emailaddress`: User's email address
    /// - `name`: User's display name
    /// - `role`: User's role in the system
    /// </remarks>
    /// <returns>Current user information extracted from JWT claims</returns>
    /// <response code="200">Successfully retrieved user information.</response>
    /// <response code="401">Unauthorized. Valid JWT token required.</response>
    [HttpGet("user-info")]
    [Authorize(Policy = "AuthenticatedUser")]
    [ProducesResponseType(typeof(object), 200)]
    public IActionResult UserInfoEndpoint()
    {
        var claims = User.Claims.Select(c => new { type = c.Type, value = c.Value }).ToList();
        
        return Ok(new 
        { 
            message = "Current user information",
            claims = claims
        });
    }
}
