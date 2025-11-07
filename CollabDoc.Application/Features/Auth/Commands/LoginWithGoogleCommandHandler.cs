using CollabDoc.Application.Dtos;
using CollabDoc.Application.Dtos.Auth;
using CollabDoc.Application.Interfaces;
using CollabDoc.Domain.Entities;
using Google.Apis.Auth;
using MediatR;
using Microsoft.Extensions.Logging;

namespace CollabDoc.Application.Features.Auth.Commands;

public class LoginWithGoogleCommandHandler : IRequestHandler<LoginWithGoogleCommand, ResLoginDto>
{
    private readonly IUserRepository _userRepository;
    private readonly IJwtTokenService _jwtService;
    private readonly IPasswordHasher _passwordHasher;
    private readonly ILogger<LoginWithGoogleCommandHandler> _logger;

    public LoginWithGoogleCommandHandler(
        IUserRepository userRepository,
        IJwtTokenService jwtService,
        IPasswordHasher passwordHasher,
        ILogger<LoginWithGoogleCommandHandler> logger)
    {
        _userRepository = userRepository;
        _jwtService = jwtService;
        _passwordHasher = passwordHasher;
        _logger = logger;
    }

    public async Task<ResLoginDto> Handle(LoginWithGoogleCommand request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Processing Google login request");

        try
        {
            // 1️⃣ Validate Google token
            var payload = await GoogleJsonWebSignature.ValidateAsync(
                request.AccessToken,
                new GoogleJsonWebSignature.ValidationSettings()
            );

            if (payload == null || string.IsNullOrWhiteSpace(payload.Email))
            {
                _logger.LogWarning("Invalid Google token provided");
                throw new ArgumentException("Token Google không hợp lệ");
            }

            var userEmail = payload.Email;
            var userName = payload.Name ?? userEmail.Split('@')[0];

            _logger.LogInformation("Google token validated for email: {Email}", userEmail);

            // 2️⃣ Get existing user or create new one
            var user = await _userRepository.GetByEmailAsync(userEmail);

            if (user == null)
            {
                // Create new user for Google login
                user = new User
                {
                    Email = userEmail,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };

                user.Password = _passwordHasher.HashPassword(Guid.NewGuid().ToString()); // Random password

                await _userRepository.CreateAsync(user);

                _logger.LogInformation("Created new user for Google login: {UserId}", user.Id);
            }
            else
            {
                _logger.LogInformation("Existing user found for Google login: {UserId}", user.Id);
            }

            // 3️⃣ Generate tokens
            var accessToken = _jwtService.GenerateAccessToken(user);
            var refreshToken = _jwtService.GenerateRefreshTokenJwt(user.Email);

            // 4️⃣ Update refresh token in database
            await _userRepository.UpdateRefreshTokenAsync(user.Id, refreshToken);

            _logger.LogInformation("Successfully generated tokens for Google user: {UserId}", user.Id);

            return new ResLoginDto
            {
                User = new UserResponseDto
                {
                    Id = user.Id,
                    Email = user.Email,
                    CreatedAt = user.CreatedAt
                },
                AccessToken = accessToken,
                RefreshToken = refreshToken,
            };
        }
        catch (InvalidJwtException ex)
        {
            _logger.LogWarning(ex, "Invalid Google JWT token");
            throw new ArgumentException("Mã token Google không hợp lệ", ex);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during Google login process");
            throw new InvalidOperationException("Lỗi trong quá trình xác thực Google", ex);
        }
    }
}