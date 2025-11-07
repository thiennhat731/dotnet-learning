using CollabDoc.Application.Dtos.Auth;
using CollabDoc.Application.Features.Auth.Commands;
using CollabDoc.Application.Interfaces;
using MediatR;


public class LoginCommandHandler : IRequestHandler<LoginCommand, ResLoginDto>
{
    private readonly IUserRepository _repository;
    private readonly IJwtTokenService _jwtTokenService;

    private readonly IPasswordHasher _passwordHasher;

    public LoginCommandHandler(IUserRepository repository, IJwtTokenService jwtTokenService, IPasswordHasher passwordHasher)
    {
        _repository = repository;
        _jwtTokenService = jwtTokenService;
        _passwordHasher = passwordHasher;
    }

    public async Task<ResLoginDto> Handle(LoginCommand request, CancellationToken cancellationToken)
    {
        // 1. Validate đầu vào
        if (string.IsNullOrWhiteSpace(request.UserLoginDto.Email) || string.IsNullOrWhiteSpace(request.UserLoginDto.Password))
            throw new ArgumentException("Email và mật khẩu phải được cung cấp.");

        // 2. Tìm user theo email
        var user = await _repository.GetByEmailAsync(request.UserLoginDto.Email)
            ?? throw new KeyNotFoundException("Email hoặc mật khẩu không hợp lệ.");

        // 3. Kiểm tra password
        if (!_passwordHasher.VerifyPassword(request.UserLoginDto.Password, user.Password))
            throw new UnauthorizedAccessException("Email hoặc mật khẩu không hợp lệ.");

        // 4. Sinh Access Token và Refresh Token
        var accessToken = _jwtTokenService.GenerateAccessToken(user);
        var refreshToken = _jwtTokenService.GenerateRefreshTokenJwt(user.Email);

        // 5. Cập nhật Refresh Token vào DB
        user.RefreshToken = refreshToken;
        user.UpdatedAt = DateTime.UtcNow;
        await _repository.UpdateAsync(user.Id, user);
        return new ResLoginDto
        {
            User = new CollabDoc.Application.Dtos.UserResponseDto
            {
                Id = user.Id,
                Email = user.Email,
                CreatedAt = user.CreatedAt,
                UpdatedAt = user.UpdatedAt
            },
            AccessToken = accessToken,
            RefreshToken = refreshToken
        };
    }
}