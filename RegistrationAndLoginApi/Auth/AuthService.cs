using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using RegistrationAndLoginApi.DataAccess;
using RegistrationAndLoginApi.DataAccess.Entities;

namespace RegistrationAndLoginApi.Auth;

public class AuthService(IOptions<JwtOptions> options, AppDbContext dbContext) : IAuthService
{
    public (string accessToken, string refreshToken) IssueTokens(User user)
    {
        var claims = new List<Claim>
        {
            new (ClaimTypes.NameIdentifier, user.Id.ToString()),
            new (ClaimTypes.Name, user.Username),
            new (ClaimTypes.Role, user.Role)
        };
        
        var signingKey = new SymmetricSecurityKey(
            Encoding.UTF8.GetBytes(options.Value.SigningKey));

        var accessTokenPreset = new JwtSecurityToken(
            issuer: options.Value.Issuer,
            audience: options.Value.Audience,
            claims: claims,
            expires: DateTime.UtcNow.AddMinutes(options.Value.AccessTokenLifetimeMinutes),
            signingCredentials: new SigningCredentials(signingKey, SecurityAlgorithms.HmacSha256));
        
        // For simplicity, refresh tokens are JWTs here,
        // but in production I’d store opaque refresh tokens in the database to allow revocation.
        var refreshTokenPreset = new JwtSecurityToken(
            issuer: options.Value.Issuer,
            audience: options.Value.Audience,
            claims: [new Claim(ClaimTypes.NameIdentifier, user.Id.ToString())],
            expires: DateTime.UtcNow.AddDays(options.Value.RefreshTokenLifetimeDays),
            signingCredentials: new SigningCredentials(signingKey, SecurityAlgorithms.HmacSha256));

        var tokenHandler = new JwtSecurityTokenHandler();
        
        string accessToken = tokenHandler.WriteToken(accessTokenPreset) 
                              ?? throw new InvalidOperationException("Something went wrong generating access token");
        
        string refreshToken = tokenHandler.WriteToken(refreshTokenPreset)
                               ?? throw new InvalidOperationException("Something went wrong generating refresh token");


        return (accessToken, refreshToken);
    }

    public (string accessToken, string refreshToken) RefreshTokens(string refreshToken)
    {
        var tokenHandler = new JwtSecurityTokenHandler();

        var validationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = options.Value.Issuer,
            ValidAudience = options.Value.Audience,
            IssuerSigningKey = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(options.Value.SigningKey))
        };

        ClaimsPrincipal principal;

        try
        {
            principal = tokenHandler.ValidateToken(refreshToken, validationParameters, out _);
        }
        catch (SecurityTokenException)
        {
            throw new UnauthorizedAccessException("Invalid refresh token");
        }

        Claim userIdClaim = principal.FindFirst(ClaimTypes.NameIdentifier)
                            ?? throw new UnauthorizedAccessException("Invalid refresh token payload");

        Guid userId = Guid.Parse(userIdClaim.Value);
        
        User user = dbContext.Users.SingleOrDefault(u => u.Id == userId)
                    ?? throw new UnauthorizedAccessException("User not found");

        return IssueTokens(user);
    }
}