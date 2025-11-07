using System.IdentityModel.Tokens.Jwt;
using CollabDoc.Application.Dtos.Auth;
using CollabDoc.Application.Features.Auth.Commands;
using CollabDoc.Application.Interfaces;
using MediatR;
using Microsoft.Extensions.Logging;

public class RefreshTokenHandler : IRequestHandler<RefreshTokenCommand, ResLoginDto>
{
    private readonly IJwtTokenService _jwtService;
    private readonly IUserRepository _userRepository;
    private readonly ILogger<RefreshTokenHandler> _logger;

    public RefreshTokenHandler(IJwtTokenService jwtService, IUserRepository userRepository, ILogger<RefreshTokenHandler> logger)
    {
        _jwtService = jwtService;
        _userRepository = userRepository;
        _logger = logger;
    }

    public async Task<ResLoginDto> Handle(RefreshTokenCommand request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Processing refresh token request");

        //  Validate refresh token format và signature
        var validated = _jwtService.ValidateToken(request.RefreshToken, validateLifetime: true);
        if (validated == null)
        {
            _logger.LogWarning("Invalid or expired refresh token provided");
            throw new UnauthorizedAccessException("Invalid or expired refresh token");
        }

        // Kiểm tra đây có phải refresh token không
        if (!_jwtService.IsRefreshToken(request.RefreshToken))
        {
            _logger.LogWarning("Token is not a refresh token");
            throw new UnauthorizedAccessException("Invalid token type");
        }

        // Extract email từ token
        var email = validated.Claims.FirstOrDefault(c => c.Type == JwtRegisteredClaimNames.Email)?.Value;
        if (string.IsNullOrWhiteSpace(email))
        {
            _logger.LogWarning("Refresh token missing email claim");
            throw new UnauthorizedAccessException("Invalid refresh token payload");
        }

        //Lấy user từ database
        var user = await _userRepository.GetByEmailAsync(email);
        if (user == null)
        {
            _logger.LogWarning("User not found for email: {Email}", email);
            throw new UnauthorizedAccessException("User not found");
        }

        // Kiểm tra refresh token có match với DB không
        if (user.RefreshToken != request.RefreshToken)
        {
            _logger.LogWarning("Refresh token mismatch for user: {UserId}", user.Id);
            throw new UnauthorizedAccessException("Refresh token không hợp lệ hoặc đã bị thu hồi");
        }

        // 6️⃣ Generate new tokens
        var newAccessToken = _jwtService.GenerateAccessToken(user);
        var newRefreshToken = _jwtService.GenerateRefreshTokenJwt(user.Email);

        // 7️⃣ Update refresh token trong database
        await _userRepository.UpdateRefreshTokenAsync(user.Id, newRefreshToken);

        _logger.LogInformation("Successfully refreshed tokens for user: {UserId}", user.Id);

        return new ResLoginDto
        {
            User = new CollabDoc.Application.Dtos.UserResponseDto
            {
                Id = user.Id,
                Email = user.Email,
                CreatedAt = user.CreatedAt,
                UpdatedAt = user.UpdatedAt
            },
            AccessToken = newAccessToken,
            RefreshToken = newRefreshToken
        };
    }
}