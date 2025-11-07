using MediatR;
namespace CollabDoc.Application.Features.Users.Commands;

public record DeleteUserCommand(string Id) : IRequest<Unit>;