namespace CollabDoc.Application.Features.Users.Queries;

using AutoMapper;
using CollabDoc.Application.Dtos;
using CollabDoc.Application.Interfaces;
using MediatR;
public class GetAllUsersHandler : IRequestHandler<GetAllUsersQuery, List<UserResponseDto>>
{
    private readonly IUserRepository _userRepository;
    private readonly IMapper _mapper;

    public GetAllUsersHandler(IUserRepository userRepository, IMapper mapper)
    {
        _userRepository = userRepository;
        _mapper = mapper;
    }

    public async Task<List<UserResponseDto>> Handle(GetAllUsersQuery request, CancellationToken cancellationToken)
    {
        var users = await _userRepository.GetAllAsync();
        return _mapper.Map<List<UserResponseDto>>(users);
    }
}