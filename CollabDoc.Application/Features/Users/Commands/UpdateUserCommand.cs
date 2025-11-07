namespace CollabDoc.Application.Features.Users.Commands;

using CollabDoc.Application.Dtos;
using MediatR;
public record UpdateUserCommand(UpdateUserRequest User) : IRequest<UserResponseDto>;