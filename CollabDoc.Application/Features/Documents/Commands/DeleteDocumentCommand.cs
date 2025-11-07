using MediatR;

namespace CollabDoc.Application.Features.Documents.Commands.DeleteDocument;

public record DeleteDocumentCommand(string Id) : IRequest;
