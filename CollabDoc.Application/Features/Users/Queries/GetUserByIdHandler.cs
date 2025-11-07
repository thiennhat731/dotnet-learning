
using AutoMapper;
using CollabDoc.Application.Dtos;
using CollabDoc.Application.Interfaces;
using MediatR;

namespace CollabDoc.Application.Features.Users.Queries;

public class GetUserByIdHandler : IRequestHandler<GetUserByIdQuery, UserResponseDto>
{
    private readonly IUserRepository _repository;
    private readonly IMapper _mapper;

    public GetUserByIdHandler(IUserRepository repository, IMapper mapper)
    {
        _repository = repository;
        _mapper = mapper;
    }

    public async Task<UserResponseDto> Handle(GetUserByIdQuery request, CancellationToken cancellationToken)
    {
        var user = await _repository.GetByIdAsync(request.Id);
        if (user is null)
            throw new KeyNotFoundException($"Không tìm thấy người dùng với ID: {request.Id}");
        user.Password = null!; // Không trả về mật khẩu
        return _mapper.Map<UserResponseDto>(user);
    }
}