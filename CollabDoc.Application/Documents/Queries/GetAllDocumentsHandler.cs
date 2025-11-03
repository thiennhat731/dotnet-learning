using CollabDoc.Application.Dtos;
using CollabDoc.Application.Interfaces;
using CollabDoc.Domain.Entities;
using MediatR;

namespace CollabDoc.Application.Documents.Queries.GetAllDocuments;

public class GetAllDocumentsHandler : IRequestHandler<GetAllDocumentsQuery, PaginationResponse<Document>>
{
    private readonly IDocumentRepository _repository;

    public GetAllDocumentsHandler(IDocumentRepository repository)
    {
        _repository = repository;
    }

    public async Task<PaginationResponse<Document>> Handle(GetAllDocumentsQuery request, CancellationToken cancellationToken)
    {
        if (string.IsNullOrEmpty(request.UserId))
            throw new UnauthorizedAccessException("User not authenticated.");

        var total = await _repository.CountAsync(request.Keyword, request.UserId);
        var skip = (request.Page - 1) * request.PageSize;

        var items = await _repository.GetPagedAsync(skip, request.PageSize, request.SortBy, request.IsDescending, request.Keyword, request.UserId);
        return new PaginationResponse<Document>(items, request.Page, request.PageSize, total);
    }
}
