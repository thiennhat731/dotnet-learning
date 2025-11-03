using CollabDoc.Domain.Entities;
using MediatR;

namespace CollabDoc.Application.Documents.Queries.GetAllDocuments;

public record GetAllDocumentsQuery(
    int Page,
    int PageSize,
    string SortBy,
    bool IsDescending,
    string? Keyword,
    string UserId
) : IRequest<PaginationResponse<Document>>;
