using MediatR;

namespace CollabDoc.Application.Documents.Commands.DeleteDocument;

public record DeleteDocumentCommand(string Id) : IRequest;
