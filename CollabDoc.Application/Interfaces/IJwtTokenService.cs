using System.Security.Claims;
using CollabDoc.Domain.Entities;

namespace CollabDoc.Application.Interfaces;

public interface IJwtTokenService
{
    /// <summary>
    /// Generate access token for user
    /// </summary>
    string GenerateAccessToken(User user);

    /// <summary>
    /// Generate JWT refresh token
    /// </summary>
    string GenerateRefreshTokenJwt(string email);


    /// <summary>
    /// Validate token and return claims principal
    /// </summary>
    ClaimsPrincipal? ValidateToken(string token, bool validateLifetime = true);

    /// <summary>
    /// Extract email from token without lifetime validation
    /// </summary>
    string? GetEmailFromToken(string token);

    /// <summary>
    /// Extract user ID from token without lifetime validation
    /// </summary>
    string? GetUserIdFromToken(string token);

    /// <summary>
    /// Check if token is expired
    /// </summary>
    bool IsTokenExpired(string token);

    /// <summary>
    /// Get remaining time before token expires
    /// </summary>
    TimeSpan? GetTokenRemainingTime(string token);

    /// <summary>
    /// Check if token is a refresh token
    /// </summary>
    bool IsRefreshToken(string token);
}