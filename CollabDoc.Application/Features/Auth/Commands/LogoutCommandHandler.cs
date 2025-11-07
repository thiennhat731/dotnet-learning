
using CollabDoc.Application.Interfaces;
using MediatR;
using Microsoft.Extensions.Logging;
namespace CollabDoc.Application.Features.Auth.Commands;

public class LogoutCommandHandler : IRequestHandler<LogoutCommand, Unit>
{
    private readonly IUserRepository _userRepository;
    private readonly ILogger<LogoutCommandHandler> _logger;
    private readonly IJwtTokenService _jwtService;

    public LogoutCommandHandler(IUserRepository userRepository, ILogger<LogoutCommandHandler> logger, IJwtTokenService jwtService)
    {
        _userRepository = userRepository;
        _logger = logger;
        _jwtService = jwtService;
    }

    public async Task<Unit> Handle(LogoutCommand request, CancellationToken cancellationToken)
    {
        var validated = _jwtService.ValidateToken(request.RefreshToken, validateLifetime: false);
        if (validated == null)
        {
            _logger.LogWarning("Invalid refresh token during logout.");
            throw new UnauthorizedAccessException("Refresh token is invalid.");
        }
        var userId = validated.Claims.FirstOrDefault(c => c.Type == "id")?.Value;
        if (string.IsNullOrEmpty(userId))
        {
            _logger.LogWarning("User ID claim missing in refresh token during logout.");
            throw new UnauthorizedAccessException("Invalid token claims.");
        }
        await _userRepository.UpdateRefreshTokenAsync(userId, string.Empty);
        return Unit.Value;
    }
}