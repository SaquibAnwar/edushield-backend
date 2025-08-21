using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using EduShield.Api.Auth;
using System.Security.Claims;

namespace EduShield.Api.Controllers;

[ApiController]
[Route("api/v1/test")]
[Authorize]
public class TestController : ControllerBase
{
    [HttpGet("public")]
    [AllowAnonymous]
    public IActionResult PublicEndpoint()
    {
        return Ok(new { message = "This is a public endpoint - no authentication required" });
    }

    [HttpGet("authenticated")]
    public IActionResult AuthenticatedEndpoint()
    {
        var userEmail = User.FindFirst(ClaimTypes.Email)?.Value;
        var userRole = User.FindFirst(ClaimTypes.Role)?.Value;
        
        return Ok(new { 
            message = "This endpoint requires authentication",
            user = new { email = userEmail, role = userRole }
        });
    }

    [HttpGet("admin-only")]
    [Authorize(Policy = AuthorizationPolicies.AdminOnly)]
    public IActionResult AdminOnlyEndpoint()
    {
        var userEmail = User.FindFirst(ClaimTypes.Email)?.Value;
        return Ok(new { 
            message = "This endpoint is for admins only",
            user = userEmail
        });
    }

    [HttpGet("student-only")]
    [Authorize(Policy = AuthorizationPolicies.StudentOnly)]
    public IActionResult StudentOnlyEndpoint()
    {
        var userEmail = User.FindFirst(ClaimTypes.Email)?.Value;
        return Ok(new { 
            message = "This endpoint is for students only",
            user = userEmail
        });
    }

    [HttpGet("faculty-only")]
    [Authorize(Policy = AuthorizationPolicies.FacultyOnly)]
    public IActionResult FacultyOnlyEndpoint()
    {
        var userEmail = User.FindFirst(ClaimTypes.Email)?.Value;
        return Ok(new { 
            message = "This endpoint is for faculty only",
            user = userEmail
        });
    }

    [HttpGet("parent-only")]
    [Authorize(Policy = AuthorizationPolicies.ParentOnly)]
    public IActionResult ParentOnlyEndpoint()
    {
        var userEmail = User.FindFirst(ClaimTypes.Email)?.Value;
        return Ok(new { 
            message = "This endpoint is for parents only",
            user = userEmail
        });
    }

    [HttpGet("dev-auth-only")]
    [Authorize(Policy = AuthorizationPolicies.DevAuthOnly)]
    public IActionResult DevAuthOnlyEndpoint()
    {
        var userEmail = User.FindFirst(ClaimTypes.Email)?.Value;
        return Ok(new { 
            message = "This endpoint is for dev auth users only",
            user = userEmail
        });
    }

    [HttpGet("admin-or-faculty")]
    [Authorize(Policy = AuthorizationPolicies.AdminOrFaculty)]
    public IActionResult AdminOrFacultyEndpoint()
    {
        var userEmail = User.FindFirst(ClaimTypes.Email)?.Value;
        var userRole = User.FindFirst(ClaimTypes.Role)?.Value;
        
        return Ok(new { 
            message = "This endpoint is for admins or faculty",
            user = new { email = userEmail, role = userRole }
        });
    }

    [HttpGet("user-info")]
    public IActionResult GetUserInfo()
    {
        var claims = User.Claims.Select(c => new { c.Type, c.Value }).ToList();
        
        return Ok(new { 
            message = "Current user information",
            claims = claims
        });
    }
}
