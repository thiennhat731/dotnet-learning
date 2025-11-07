
using CollabDoc.Application.Dtos;
using MediatR;
namespace CollabDoc.Application.Features.Users.Queries;

public record GetUserByIdQuery(string Id) : IRequest<UserResponseDto>;