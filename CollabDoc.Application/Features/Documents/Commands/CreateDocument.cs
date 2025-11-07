using CollabDoc.Application.Dtos;
using CollabDoc.Domain.Entities;
using MediatR;

namespace CollabDoc.Application.Features.Documents.Commands.CreateDocument;

public record CreateDocumentCommand(DocumentCreateDto Dto, string OwnerId) : IRequest<Document>;
