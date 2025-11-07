namespace CollabDoc.Application.Features.Users.Commands;

using AutoMapper;
using CollabDoc.Application.Dtos;
using CollabDoc.Application.Interfaces;
using MediatR;
public class UpdateUserHandlers : IRequestHandler<UpdateUserCommand, UserResponseDto>
{
    private readonly IUserRepository _repo;
    private readonly IMapper _mapper;
    public UpdateUserHandlers(IUserRepository repo, IMapper mapper)
    {
        _mapper = mapper;
        _repo = repo;
    }
    public async Task<UserResponseDto> Handle(UpdateUserCommand request, CancellationToken cancellationToken)
    {
        var user = await _repo.GetByEmailAsync(request.User.Email);
        if (user is null)
            throw new KeyNotFoundException($"Không tìm thấy người dùng với Email: {request.User.Email}");
        user.Email = request.User.Email;
        user.UpdatedAt = DateTime.UtcNow;
        await _repo.UpdateAsync(user.Id, user);
        return _mapper.Map<UserResponseDto>(user);
    }
}