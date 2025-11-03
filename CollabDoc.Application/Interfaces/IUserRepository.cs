using CollabDoc.Domain.Entities;

namespace CollabDoc.Application.Interfaces;

public interface IUserRepository
{
    Task<List<User>> GetAllAsync();
    Task<User?> GetByIdAsync(string id);
    Task<User?> GetByEmailAsync(string email);
    Task CreateAsync(User user);
    Task UpdateAsync(string id, User user);
    Task DeleteAsync(string id);
    Task UpdateRefreshTokenAsync(string id, string refreshToken);
    Task<bool> ExistsByEmailAsync(string email);
}
