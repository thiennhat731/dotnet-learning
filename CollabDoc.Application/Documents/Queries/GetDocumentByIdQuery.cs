using CollabDoc.Domain.Entities;
using MediatR;

namespace CollabDoc.Application.Documents.Queries.GetDocumentById;

public record GetDocumentByIdQuery(string Id) : IRequest<Document>;
