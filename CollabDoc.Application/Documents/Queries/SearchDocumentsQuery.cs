using CollabDoc.Domain.Entities;
using MediatR;

namespace CollabDoc.Application.Documents.Queries.SearchDocuments;

public record SearchDocumentsQuery(string Keyword) : IRequest<List<Document>>;
