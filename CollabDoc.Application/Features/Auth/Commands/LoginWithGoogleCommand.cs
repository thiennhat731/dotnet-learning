using CollabDoc.Application.Dtos.Auth;
using MediatR;

namespace CollabDoc.Application.Features.Auth.Commands;

public record LoginWithGoogleCommand(string AccessToken) : IRequest<ResLoginDto>;