using CollabDoc.Application.Dtos;
using CollabDoc.Application.Interfaces;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;

namespace CollabDoc.Application.Features.Auth.Commands;

public class RegisterCommandHandler : IRequestHandler<RegisterCommand, UserResponseDto>
{
    private readonly IUserRepository _userRepository;
    private readonly ILogger<RegisterCommandHandler> _logger;
    private readonly IPasswordHasher _passwordHasher;

    public RegisterCommandHandler(IPasswordHasher passwordHasher, IUserRepository userRepository, ILogger<RegisterCommandHandler> logger)
    {
        _userRepository = userRepository;
        _logger = logger;
        _passwordHasher = passwordHasher;
    }

    public async Task<UserResponseDto> Handle(RegisterCommand request, CancellationToken cancellationToken)
    {
        if (await _userRepository.ExistsByEmailAsync(request.UserRegisterDto.Email))
            throw new InvalidOperationException("Email đã được sử dụng.");

        var hashed = _passwordHasher.HashPassword(request.UserRegisterDto.Password);
        var user = new Domain.Entities.User
        {
            Email = request.UserRegisterDto.Email,
            Password = hashed,
            CreatedAt = DateTime.UtcNow
        };

        await _userRepository.CreateAsync(user);
        return new UserResponseDto
        {
            Id = user.Id,
            Email = user.Email,
            CreatedAt = user.CreatedAt,
            UpdatedAt = user.UpdatedAt
        };
    }
}