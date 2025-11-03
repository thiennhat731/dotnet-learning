using CollabDoc.Domain.Entities;
using MediatR;

namespace CollabDoc.Application.Documents.Commands.UpdateDocument;

public record UpdateDocumentCommand(string Id, Document Document) : IRequest;
