using System.IdentityModel.Tokens.Jwt;
using System.Net.Http.Json;
using EduShield.Core.Configuration;
using Microsoft.Extensions.Options;

namespace EduShield.Core.Services;

public class GoogleAuthService : IGoogleAuthService
{
    private readonly HttpClient _httpClient;
    private readonly GoogleSettings _googleSettings;

    public GoogleAuthService(HttpClient httpClient, IOptions<AuthenticationConfiguration> authConfig)
    {
        _httpClient = httpClient;
        _googleSettings = authConfig.Value.Google;
    }

    public Task<GoogleUserInfo?> VerifyGoogleTokenAsync(string idToken)
    {
        try
        {
            // In production, you should verify the token signature
            // For now, we'll decode the JWT to extract claims
            var handler = new JwtSecurityTokenHandler();
            var token = handler.ReadJwtToken(idToken);

            var claims = token.Claims.ToDictionary(c => c.Type, c => c.Value, StringComparer.OrdinalIgnoreCase);

            if (!claims.ContainsKey("sub") || !claims.ContainsKey("email"))
                return Task.FromResult<GoogleUserInfo?>(null);

            var userInfo = new GoogleUserInfo
            {
                Sub = claims["sub"],
                Email = claims["email"],
                Name = claims.TryGetValue("name", out var name) ? name : "",
                Picture = claims.TryGetValue("picture", out var picture) ? picture : null,
                EmailVerified = claims.TryGetValue("email_verified", out var emailVerified) && bool.TryParse(emailVerified, out var verified) && verified
            };

            return Task.FromResult<GoogleUserInfo?>(userInfo);
        }
        catch
        {
            return Task.FromResult<GoogleUserInfo?>(null);
        }
    }
}
