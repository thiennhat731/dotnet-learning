
using CollabDoc.Application.Dtos.Auth;
using MediatR;
namespace CollabDoc.Application.Features.Auth.Commands;

public record LoginCommand(UserLoginDto UserLoginDto) : IRequest<ResLoginDto>;