using CollabDoc.Application.Dtos;
using CollabDoc.Application.Dtos.Auth;
using MediatR;
namespace CollabDoc.Application.Features.Auth.Commands;

public record RegisterCommand(UserRegisterDto UserRegisterDto) : IRequest<UserResponseDto>;