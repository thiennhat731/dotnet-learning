using AutoMapper;
using CollabDoc.Application.Dtos;
using CollabDoc.Domain.Entities;

namespace CollabDoc.Application.Mappings;

public class MappingProfile : Profile
{
    public MappingProfile()
    {
        CreateMap<CreateUserRequest, User>(); // Request -> Entity
        CreateMap<User, UserResponseDto>();  // Entity -> Response

    }
}
