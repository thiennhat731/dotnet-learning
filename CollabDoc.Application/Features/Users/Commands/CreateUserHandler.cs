namespace CollabDoc.Application.Features.Users.Commands;

using AutoMapper;
using CollabDoc.Application.Dtos;
using CollabDoc.Application.Interfaces;
using CollabDoc.Domain.Entities;
using MediatR;

public class CreateUserHandler : IRequestHandler<CreateUserCommand, UserResponseDto>
{
    private readonly IUserRepository _repository;
    private readonly IMapper _mapper;

    private readonly IPasswordHasher _passwordHasher;
    public CreateUserHandler(IUserRepository repository, IMapper mapper, IPasswordHasher passwordHasher)
    {
        _repository = repository;
        _mapper = mapper;
        _passwordHasher = passwordHasher;
    }

    public async Task<UserResponseDto> Handle(CreateUserCommand request, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.Request.Email))
            throw new ArgumentException("Email không được để trống.");

        var existing = await _repository.GetByEmailAsync(request.Request.Email);
        if (existing is not null)
            throw new InvalidOperationException("Email đã tồn tại.");
        var user = new User
        {
            Email = request.Request.Email,
            Password = _passwordHasher.HashPassword(request.Request.Password),
            CreatedAt = DateTime.UtcNow
        };
        user.UpdatedAt = DateTime.UtcNow;
        await _repository.CreateAsync(user);
        return _mapper.Map<UserResponseDto>(user);
    }
}