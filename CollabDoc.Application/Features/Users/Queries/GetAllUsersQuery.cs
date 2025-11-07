namespace CollabDoc.Application.Features.Users.Queries;

using CollabDoc.Application.Dtos;
using CollabDoc.Domain.Entities;
using MediatR;
public record GetAllUsersQuery : IRequest<List<UserResponseDto>>;
