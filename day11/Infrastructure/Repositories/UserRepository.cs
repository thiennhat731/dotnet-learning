using day11.Domain.Entities;
using day11.Application.Interfaces;
using day11.Infrastructure.Settings;
using MongoDB.Driver;

namespace day11.Infrastructure.Repositories;

public class UserRepository : IUserRepository
{
    private readonly IMongoCollection<User> _users;

    public UserRepository(MongoDbSettings settings)
    {
        var client = new MongoClient(settings.ConnectionString);
        var database = client.GetDatabase(settings.DatabaseName);
        _users = database.GetCollection<User>("users");
    }

    public async Task<List<User>> GetAllAsync() =>
        await _users.Find(_ => true).ToListAsync();

    public async Task<User?> GetByIdAsync(string id) =>
        await _users.Find(u => u.Id == id).FirstOrDefaultAsync();

    public async Task<User?> GetByEmailAsync(string email) =>
        await _users.Find(u => u.Email == email).FirstOrDefaultAsync();

    public async Task CreateAsync(User user) =>
        await _users.InsertOneAsync(user);

    public async Task UpdateAsync(string id, User user) =>
        await _users.ReplaceOneAsync(u => u.Id == id, user);

    public async Task DeleteAsync(string id) =>
        await _users.DeleteOneAsync(u => u.Id == id);

    // ✅ Thêm helper để update refresh token riêng
    public async Task UpdateRefreshTokenAsync(string id, string refreshToken)
    {
        var update = Builders<User>.Update.Set(u => u.RefreshToken, refreshToken);
        await _users.UpdateOneAsync(u => u.Id == id, update);
    }

    public async Task<bool> ExistsByEmailAsync(string email) =>
        await _users.Find(u => u.Email == email).AnyAsync();

    // Implement the missing RegisterAsync method
    public async Task<User> RegisterAsync(string email, string hashedPassword)
    {
        var user = new User
        {
            Email = email,
            Password = hashedPassword
        };

        await _users.InsertOneAsync(user);
        return user;
    }
}
