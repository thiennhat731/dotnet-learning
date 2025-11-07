using CollabDoc.Application.Dtos;
using MediatR;
namespace CollabDoc.Application.Features.Users.Commands;

public record CreateUserCommand(CreateUserRequest Request) : IRequest<UserResponseDto>;