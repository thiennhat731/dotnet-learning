using CollabDoc.Application.Dtos;
using MediatR;

namespace CollabDoc.Application.Features.Auth.Queries;

public record GetCurrentUserQuery(string Email) : IRequest<UserResponseDto>;