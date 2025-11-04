using day11.Domain.Entities;

namespace day11.Application.Interfaces;

public interface IUserRepository
{
    Task<User?> GetByEmailAsync(string email);
    Task UpdateRefreshTokenAsync(string userId, string refreshToken);
    Task<User> RegisterAsync(string email, string password);
}
