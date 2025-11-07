using CollabDoc.Application.Dtos.Auth;
using MediatR;

namespace CollabDoc.Application.Features.Auth.Commands;

public record RefreshTokenCommand(string RefreshToken) : IRequest<ResLoginDto>;