using MediatR;

namespace CollabDoc.Application.Features.Auth.Commands;

public record LogoutCommand(string RefreshToken) : IRequest<Unit>;