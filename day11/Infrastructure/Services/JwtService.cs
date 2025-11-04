using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using day11.Application.Interfaces;
using day11.Domain.Entities;
using Microsoft.IdentityModel.Tokens;

namespace day11.Infrastructure.Services;

public class JwtService : IJwtService
{
    private readonly string _key = "super-secret-key-demo";
    private readonly string _issuer = "collabdoc-api";
    private readonly string _audience = "collabdoc-client";

    public string GenerateAccessToken(User user)
    {
        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, user.Id),
            new Claim(ClaimTypes.Email, user.Email)
        };

        var creds = new SigningCredentials(new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_key)),
                                           SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(_issuer, _audience, claims,
            expires: DateTime.UtcNow.AddMinutes(30), signingCredentials: creds);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    public string GenerateRefreshToken(string email)
    {
        var claims = new[] { new Claim(JwtRegisteredClaimNames.Email, email) };
        var creds = new SigningCredentials(new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_key)),
                                           SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(_issuer, _audience, claims,
            expires: DateTime.UtcNow.AddDays(7), signingCredentials: creds);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    public ClaimsPrincipal? ValidateToken(string token, bool validateLifetime)
    {
        var tokenHandler = new JwtSecurityTokenHandler();
        var validationParams = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateIssuerSigningKey = true,
            ValidateLifetime = validateLifetime,
            ValidIssuer = _issuer,
            ValidAudience = _audience,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_key))
        };

        try
        {
            return tokenHandler.ValidateToken(token, validationParams, out _);
        }
        catch
        {
            return null;
        }
    }
}
