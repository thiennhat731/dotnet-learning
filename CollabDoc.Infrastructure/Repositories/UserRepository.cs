using CollabDoc.Application.Interfaces;
using CollabDoc.Domain.Entities;
using CollabDoc.Infrastructure.Settings;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MongoDB.Driver;

namespace CollabDoc.Infrastructure.Repositories;

public class UserRepository : IUserRepository
{
    private readonly IMongoCollection<User> _users;
    private readonly MongoDbSettings _mongoSettings;
    private readonly ILogger<UserRepository> _logger;

    public UserRepository(
        IOptions<MongoDbSettings> mongoOptions,
        ILogger<UserRepository> logger)
    {
        _mongoSettings = mongoOptions.Value;
        _logger = logger;
        var client = new MongoClient(_mongoSettings.ConnectionString);
        var database = client.GetDatabase(_mongoSettings.DatabaseName);
        _users = database.GetCollection<User>(_mongoSettings.UsersCollection);
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
}
