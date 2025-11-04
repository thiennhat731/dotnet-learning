using MediatR;
using day11.Application.Dtos;
using day11.Application.Interfaces;

namespace day11.Application.Features.Auth.Commands;

public record LoginCommand(string Email, string Password) : IRequest<AuthResponseDto>;

public class LoginCommandHandler : IRequestHandler<LoginCommand, AuthResponseDto>
{
    private readonly IUserRepository _userRepository;
    private readonly IJwtService _jwtService;

    public LoginCommandHandler(IUserRepository userRepository, IJwtService jwtService)
    {
        _userRepository = userRepository;
        _jwtService = jwtService;
    }

    public async Task<AuthResponseDto> Handle(LoginCommand request, CancellationToken cancellationToken)
    {
        var user = await _userRepository.GetByEmailAsync(request.Email);
        if (user == null || user.Password != request.Password)
            throw new UnauthorizedAccessException("Email hoặc mật khẩu không hợp lệ");

        var accessToken = _jwtService.GenerateAccessToken(user);
        var refreshToken = _jwtService.GenerateRefreshToken(user.Email);

        await _userRepository.UpdateRefreshTokenAsync(user.Id, refreshToken);

        return new AuthResponseDto(user.Id, user.Email, accessToken, refreshToken);
    }
}
