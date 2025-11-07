using CollabDoc.Application.Dtos;
using CollabDoc.Application.Interfaces;
using MediatR;
namespace CollabDoc.Application.Features.Auth.Queries;

public class GetCurrentUserQueryHandler : IRequestHandler<GetCurrentUserQuery, UserResponseDto>
{
    private readonly IUserRepository _userRepository;


    public GetCurrentUserQueryHandler(IUserRepository userRepository)
    {
        _userRepository = userRepository;
    }

    public async Task<UserResponseDto> Handle(GetCurrentUserQuery request, CancellationToken cancellationToken)
    {
        // Implementation to get the current user details
        var user = await _userRepository.GetByEmailAsync(request.Email);
        if (user == null)
            throw new KeyNotFoundException("User not found");

        return new UserResponseDto
        {
            Id = user.Id,
            Email = user.Email,
            CreatedAt = user.CreatedAt,
            UpdatedAt = user.UpdatedAt
        };
    }
}