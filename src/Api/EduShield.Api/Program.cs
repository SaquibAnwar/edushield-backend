using EduShield.Core.Data;
using EduShield.Core.Interfaces;
using EduShield.Core.Security;
using EduShield.Core.Services;
using EduShield.Core.Configuration;
using EduShield.Core.Enums;
using EduShield.Api.Auth;
using EduShield.Api.Auth.Requirements;
using EduShield.Api.Auth.Handlers;
using EduShield.Api.Middleware;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using System.Reflection;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

// Add Serilog
builder.Host.UseSerilog((ctx, services, lc) => lc
    .ReadFrom.Configuration(ctx.Configuration)
    .Enrich.FromLogContext()
    .WriteTo.Console());

// Add services to the container.
builder.Services.AddControllers();

// Add HttpContextAccessor for accessing current user context
builder.Services.AddHttpContextAccessor();

// Load environment variables
builder.Configuration.AddEnvironmentVariables();

// Override configuration with environment variables
var authConfig = new AuthenticationConfiguration
{
    Jwt = new JwtSettings
    {
        SecretKey = Environment.GetEnvironmentVariable("JWT_SECRET_KEY") ?? "your-super-secret-jwt-key-with-at-least-32-characters",
        Issuer = Environment.GetEnvironmentVariable("JWT_ISSUER") ?? "EduShield",
        Audience = Environment.GetEnvironmentVariable("JWT_AUDIENCE") ?? "EduShield",
        ExpirationMinutes = int.TryParse(Environment.GetEnvironmentVariable("JWT_EXPIRATION_MINUTES"), out var expMinutes) ? expMinutes : 60,
        RefreshTokenExpirationDays = int.TryParse(Environment.GetEnvironmentVariable("JWT_REFRESH_TOKEN_EXPIRATION_DAYS"), out var refreshDays) ? refreshDays : 7
    },
    Google = new GoogleSettings
    {
        ClientId = Environment.GetEnvironmentVariable("GOOGLE_CLIENT_ID") ?? "your-google-client-id",
        ClientSecret = Environment.GetEnvironmentVariable("GOOGLE_CLIENT_SECRET") ?? "your-google-client-secret",
        RedirectUri = Environment.GetEnvironmentVariable("GOOGLE_REDIRECT_URI") ?? "http://localhost:5000/api/v1/auth/google/callback"
    },
    EnableDevAuth = bool.TryParse(Environment.GetEnvironmentVariable("ENABLE_DEV_AUTH"), out var enableDevAuth) ? enableDevAuth : true
};

// Configure Authentication
builder.Services.Configure<AuthenticationConfiguration>(options =>
{
    options.Jwt = authConfig.Jwt;
    options.Google = authConfig.Google;
    options.EnableDevAuth = authConfig.EnableDevAuth;
});

// Add JWT Authentication
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(authConfig.Jwt.SecretKey)),
            ValidateIssuer = true,
            ValidIssuer = authConfig.Jwt.Issuer,
            ValidAudience = authConfig.Jwt.Audience,
            ValidateLifetime = true,
            ClockSkew = TimeSpan.Zero
        };
    });

// Add Authorization with policies
builder.Services.AddAuthorization(options =>
{
    options.AddPolicy(AuthorizationPolicies.AdminOnly, policy => policy.RequireRole(UserRole.Admin));
    options.AddPolicy(AuthorizationPolicies.StudentOnly, policy => policy.RequireRole(UserRole.Student));
    options.AddPolicy(AuthorizationPolicies.FacultyOnly, policy => policy.RequireRole(UserRole.Faculty));
    options.AddPolicy(AuthorizationPolicies.ParentOnly, policy => policy.RequireRole(UserRole.Parent));
    options.AddPolicy(AuthorizationPolicies.DevAuthOnly, policy => policy.RequireRole(UserRole.DevAuth));
    options.AddPolicy(AuthorizationPolicies.AdminOrFaculty, policy => policy.RequireAnyRole(UserRole.Admin, UserRole.Faculty));
    options.AddPolicy(AuthorizationPolicies.AdminOrStudent, policy => policy.RequireAnyRole(UserRole.Admin, UserRole.Student));
    options.AddPolicy(AuthorizationPolicies.AdminOrParent, policy => policy.RequireAnyRole(UserRole.Admin, UserRole.Parent));
    options.AddPolicy(AuthorizationPolicies.AuthenticatedUser, policy => policy.RequireAuthenticatedUser());
    
    // Student-specific policies
    options.AddPolicy("StudentAccess", policy => 
        policy.Requirements.Add(new StudentAccessRequirement()));
});

// Add Authorization Handlers
builder.Services.AddScoped<IAuthorizationHandler, StudentAuthorizationHandler>();
builder.Services.AddScoped<IAuthorizationHandler, StudentPerformanceAuthorizationHandler>();

// Add HttpClient for Google Auth
builder.Services.AddHttpClient();

// Add Swagger/OpenAPI
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "EduShield Backend API",
        Version = "v1",
        Description = "EduShield Backend API for managing educational data",
        Contact = new OpenApiContact
        {
            Name = "EduShield Team",
            Email = "support@edushield.com"
        }
    });

    // Include XML comments from the API project
    var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
    var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
    if (File.Exists(xmlPath))
    {
        c.IncludeXmlComments(xmlPath);
    }

    // Include XML comments from the Core project
    var coreXmlFile = "EduShield.Core.xml";
    var coreXmlPath = Path.Combine(AppContext.BaseDirectory, coreXmlFile);
    if (File.Exists(coreXmlPath))
    {
        c.IncludeXmlComments(coreXmlPath);
    }
});

// Add Entity Framework - conditionally based on environment
if (builder.Environment.IsEnvironment("Test"))
{
    // Use in-memory database for testing
    builder.Services.AddDbContext<EduShieldDbContext>(options =>
        options.UseInMemoryDatabase("TestDatabase"));
}
else
{
    // Use PostgreSQL for development and production
    builder.Services.AddDbContext<EduShieldDbContext>(options =>
        options.UseNpgsql(builder.Configuration.GetConnectionString("Postgres")));
}

// Add Repositories
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<IStudentRepository, StudentRepository>();
builder.Services.AddScoped<IFacultyRepository, FacultyRepository>();
builder.Services.AddScoped<IStudentPerformanceRepository, StudentPerformanceRepository>();

// Add Services
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IGoogleAuthService, GoogleAuthService>();
builder.Services.AddScoped<IJwtService, JwtService>();
builder.Services.AddScoped<ITestDataSeeder, TestDataSeeder>();
builder.Services.AddScoped<IStudentService, StudentService>();
builder.Services.AddScoped<IFacultyService, FacultyService>();
builder.Services.AddScoped<IStudentPerformanceService, StudentPerformanceService>();
builder.Services.AddScoped<IEncryptionService, EncryptionService>();

// Add Health Checks - conditionally based on environment
if (builder.Environment.IsEnvironment("Test"))
{
    // Only add DbContext check for testing
    builder.Services.AddHealthChecks()
        .AddDbContextCheck<EduShieldDbContext>();
}
else
{
    // Add full health checks for development and production
    builder.Services.AddHealthChecks()
        .AddNpgSql(builder.Configuration.GetConnectionString("Postgres")!)
        .AddDbContextCheck<EduShieldDbContext>();
}

// Add Redis Cache (optional)
builder.Services.AddStackExchangeRedisCache(options =>
{
    options.Configuration = builder.Configuration.GetConnectionString("Redis");
});

// Configure HTTPS Redirection
builder.Services.AddHttpsRedirection(options =>
{
    options.RedirectStatusCode = StatusCodes.Status307TemporaryRedirect;
    options.HttpsPort = 5001;
});

// Add CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

var app = builder.Build();

 // Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "EduShield Backend API v1");
        c.RoutePrefix = "swagger";
    });
}

// Only use HTTPS redirection in production
if (!app.Environment.IsDevelopment())
{
    app.UseHttpsRedirection();
}

app.UseCors("AllowAll");

// Use JWT Authentication Middleware
app.UseJwtAuthentication();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

// Map Health Checks
app.MapHealthChecks("/health");

// Seed test data only when NOT in test environment
if (!app.Environment.IsEnvironment("Test"))
{
    using (var scope = app.Services.CreateScope())
    {
        var seeder = scope.ServiceProvider.GetRequiredService<ITestDataSeeder>();
        await seeder.SeedUsersAsync();
    }
}

app.Run();

// Make Program class accessible for testing
public partial class Program { }