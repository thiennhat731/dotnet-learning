using CollabDoc.Application.Dtos;
using CollabDoc.Domain.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using CollabDoc.Application.Security;
using CollabDoc.Application.Interfaces;

namespace CollabDoc.Application.Services;

public class UserService
{
    private readonly PasswordHasher<string> _passwordHasher = new();
    private readonly IUserRepository _repository;
    public readonly IConfiguration _config;
    private readonly JwtTokenService _jwtTokenService;

    public UserService(IUserRepository repository, IConfiguration config, JwtTokenService jwtTokenService)
    {
        _repository = repository;
        _config = config;
        _jwtTokenService = jwtTokenService;
    }

    public async Task<List<User>> GetAllAsync() =>
        await _repository.GetAllAsync();

    public async Task<User> GetByIdAsync(string id)
    {

        var user = await _repository.GetByIdAsync(id);
        if (user is null)
            throw new KeyNotFoundException($"Không tìm thấy người dùng với ID: {id}");
        user.Password = null!; // Không trả về mật khẩu
        return user;
    }

    public async Task CreateAsync(User user)
    {
        if (string.IsNullOrWhiteSpace(user.Email))
            throw new ArgumentException("Email không được để trống.");

        var existing = await _repository.GetByEmailAsync(user.Email);
        if (existing is not null)
            throw new InvalidOperationException("Email đã tồn tại.");
        user.Password = _passwordHasher.HashPassword(string.Empty, user.Password);// Giữ nguyên mật khẩu đã hash từ bên ngoài
        user.CreatedAt = DateTime.UtcNow;
        user.UpdatedAt = DateTime.UtcNow;
        await _repository.CreateAsync(user);
    }

    public async Task UpdateAsync(string id, User user)
    {
        var existing = await _repository.GetByIdAsync(id);
        if (existing is null)
            throw new KeyNotFoundException($"Không tìm thấy người dùng với ID: {id}");

        user.Id = existing.Id;
        user.CreatedAt = existing.CreatedAt;
        user.UpdatedAt = DateTime.UtcNow;

        await _repository.UpdateAsync(id, user);
    }

    public async Task DeleteAsync(string id)
    {
        var existing = await _repository.GetByIdAsync(id);
        if (existing is null)
            throw new KeyNotFoundException($"Không tìm thấy người dùng với ID: {id}");

        await _repository.DeleteAsync(id);
    }
    public async Task<User?> GetByEmailAsync(string email) =>
        await _repository.GetByEmailAsync(email);

    public async Task UpdateRefreshTokenAsync(string id, string refreshToken)
    {
        var existing = await _repository.GetByIdAsync(id);
        if (existing is null)
            throw new KeyNotFoundException($"Không tìm thấy người dùng với ID: {id}");

        await _repository.UpdateRefreshTokenAsync(id, refreshToken);
    }
    public async Task<ResLoginDto> LoginAsync(UserLoginDto loginDto)
    {
        // ✅ 1. Validate đầu vào
        if (string.IsNullOrWhiteSpace(loginDto.Email) || string.IsNullOrWhiteSpace(loginDto.Password))
            throw new ArgumentException("Email và mật khẩu phải được cung cấp.");

        // ✅ 2. Tìm user theo email
        var user = await _repository.GetByEmailAsync(loginDto.Email)
            ?? throw new KeyNotFoundException("Email hoặc mật khẩu không hợp lệ.");

        // ✅ 3. Kiểm tra password
        var passwordCheck = _passwordHasher.VerifyHashedPassword(string.Empty, user.Password, loginDto.Password);
        if (passwordCheck == PasswordVerificationResult.Failed)
            throw new UnauthorizedAccessException("Email hoặc mật khẩu không hợp lệ.");

        // ✅ 4. Sinh Access Token và Refresh Token
        var accessToken = _jwtTokenService.GenerateAccessToken(user);
        var refreshToken = _jwtTokenService.GenerateSecureRefreshToken();

        // ✅ 5. Cập nhật Refresh Token vào DB
        user.RefreshToken = refreshToken;
        user.UpdatedAt = DateTime.UtcNow;
        await _repository.UpdateAsync(user.Id, user);
        return new ResLoginDto
        {
            User = user,
            AccessToken = accessToken,
            RefreshToken = refreshToken
        };
    }
    public async Task<User> RegisterAsync(UserRegisterDto dto)
    {
        if (await _repository.ExistsByEmailAsync(dto.Email))
            throw new InvalidOperationException("Email đã được sử dụng.");

        var hashed = _passwordHasher.HashPassword(string.Empty, dto.Password);
        var user = new User
        {
            Email = dto.Email,
            Password = hashed,
            CreatedAt = DateTime.UtcNow
        };

        await _repository.CreateAsync(user);
        return user;
    }
    public async Task LogoutAsync(string userId)
    {
        await _repository.UpdateRefreshTokenAsync(userId, string.Empty); // thu hồi refresh token
    }
}
