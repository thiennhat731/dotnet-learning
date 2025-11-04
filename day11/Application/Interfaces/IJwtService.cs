using day11.Domain.Entities;
using System.Security.Claims;
namespace day11.Application.Interfaces;

public interface IJwtService
{
    string GenerateAccessToken(User user);
    string GenerateRefreshToken(string email);
    ClaimsPrincipal? ValidateToken(string token, bool validateLifetime);
}
