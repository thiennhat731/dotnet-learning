using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using CollabDoc.Application.Interfaces;
using CollabDoc.Domain.Entities;
using CollabDoc.Infrastructure.Settings;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace CollabDoc.Infrastructure.Security;

public class JwtTokenService : IJwtTokenService
{
    private readonly JwtSettings _jwtSettings;
    private readonly ILogger<JwtTokenService> _logger;
    private readonly SymmetricSecurityKey _signingKey;

    public JwtTokenService(
        IOptions<JwtSettings> jwtOptions, // Options Pattern
        ILogger<JwtTokenService> logger)
    {
        _jwtSettings = jwtOptions.Value;
        _logger = logger;

        _signingKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtSettings.SecretKey));
    }

    public string GenerateAccessToken(User user)
    {
        var claims = CreateUserClaims(user);

        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(claims),
            Expires = DateTime.UtcNow.AddMinutes(_jwtSettings.AccessTokenExpiryMinutes),
            Issuer = _jwtSettings.Issuer,
            Audience = _jwtSettings.Audience,
            SigningCredentials = new SigningCredentials(_signingKey, SecurityAlgorithms.HmacSha256Signature),
            TokenType = "JWT"
        };

        var tokenHandler = new JwtSecurityTokenHandler();
        var token = tokenHandler.CreateToken(tokenDescriptor);
        var tokenString = tokenHandler.WriteToken(token);

        _logger.LogDebug("Generated access token for user {UserId} with expiry {Expiry}",
            user.Id, tokenDescriptor.Expires);

        return tokenString;
    }

    public string GenerateRefreshTokenJwt(string email)
    {
        var claims = new[]
        {
            new Claim(JwtRegisteredClaimNames.Email, email),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
            new Claim(JwtRegisteredClaimNames.Iat, DateTimeOffset.UtcNow.ToUnixTimeSeconds().ToString(), ClaimValueTypes.Integer64),
            new Claim("token_type", "refresh")
        };

        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(claims),
            Expires = DateTime.UtcNow.AddDays(_jwtSettings.RefreshTokenExpiryDays),
            Issuer = _jwtSettings.Issuer,
            Audience = _jwtSettings.Audience,
            SigningCredentials = new SigningCredentials(_signingKey, SecurityAlgorithms.HmacSha256Signature)
        };

        var tokenHandler = new JwtSecurityTokenHandler();
        var token = tokenHandler.CreateToken(tokenDescriptor);
        var tokenString = tokenHandler.WriteToken(token);

        _logger.LogDebug("Generated refresh token for email {Email} with expiry {Expiry}",
            email, tokenDescriptor.Expires);

        return tokenString;
    }

    public ClaimsPrincipal? ValidateToken(string token, bool validateLifetime = true)
    {
        if (string.IsNullOrWhiteSpace(token))
        {
            _logger.LogWarning("Token validation failed: token is null or empty");
            return null;
        }

        try
        {
            var tokenHandler = new JwtSecurityTokenHandler();

            var validationParameters = new TokenValidationParameters
            {
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = _signingKey,
                ValidateIssuer = true,
                ValidIssuer = _jwtSettings.Issuer,
                ValidateAudience = true,
                ValidAudience = _jwtSettings.Audience,
                ValidateLifetime = validateLifetime,
                ClockSkew = TimeSpan.FromMinutes(_jwtSettings.ClockSkewMinutes),
                RequireExpirationTime = true,
                RequireSignedTokens = true
            };

            var principal = tokenHandler.ValidateToken(token, validationParameters, out var validatedToken);

            // Kiểm tra thêm algorithm
            if (validatedToken is JwtSecurityToken jwtToken &&
                !jwtToken.Header.Alg.Equals(SecurityAlgorithms.HmacSha256, StringComparison.InvariantCultureIgnoreCase))
            {
                _logger.LogWarning("Token validation failed: invalid algorithm {Algorithm}", jwtToken.Header.Alg);
                return null;
            }

            _logger.LogDebug("Token validated successfully for user {UserId}",
                principal.FindFirst(ClaimTypes.NameIdentifier)?.Value);

            return principal;
        }
        catch (SecurityTokenValidationException ex)
        {
            _logger.LogWarning(ex, "Token validation failed: {Reason}", ex.Message);
            return null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error during token validation");
            return null;
        }
    }

    public string? GetEmailFromToken(string token)
    {
        var principal = ValidateToken(token, validateLifetime: false);
        return principal?.FindFirst(ClaimTypes.Email)?.Value;
    }

    public string? GetUserIdFromToken(string token)
    {
        var principal = ValidateToken(token, validateLifetime: false);
        return principal?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
    }

    public bool IsTokenExpired(string token)
    {
        if (string.IsNullOrWhiteSpace(token))
            return true;

        try
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var jsonToken = tokenHandler.ReadJwtToken(token);
            var isExpired = jsonToken.ValidTo < DateTime.UtcNow;

            _logger.LogDebug("Token expiry check: {IsExpired}. Valid to: {ValidTo}",
                isExpired, jsonToken.ValidTo);

            return isExpired;
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to check token expiry");
            return true;
        }
    }

    public TimeSpan? GetTokenRemainingTime(string token)
    {
        if (string.IsNullOrWhiteSpace(token))
            return null;

        try
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var jsonToken = tokenHandler.ReadJwtToken(token);
            var remaining = jsonToken.ValidTo - DateTime.UtcNow;

            return remaining > TimeSpan.Zero ? remaining : TimeSpan.Zero;
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to get token remaining time");
            return null;
        }
    }

    public bool IsRefreshToken(string token)
    {
        var principal = ValidateToken(token, validateLifetime: false);
        return principal?.FindFirst("token_type")?.Value == "refresh";
    }

    private Claim[] CreateUserClaims(User user)
    {
        return new[]
        {
            new Claim(ClaimTypes.NameIdentifier, user.Id),
            new Claim(ClaimTypes.Email, user.Email),
            new Claim(JwtRegisteredClaimNames.Sub, user.Id),
            new Claim(JwtRegisteredClaimNames.Email, user.Email),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
            new Claim(JwtRegisteredClaimNames.Iat, DateTimeOffset.UtcNow.ToUnixTimeSeconds().ToString(), ClaimValueTypes.Integer64),
            new Claim("uid", user.Id),
            new Claim("token_type", "access")
        };
    }
}