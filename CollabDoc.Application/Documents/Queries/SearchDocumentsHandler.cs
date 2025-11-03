using CollabDoc.Application.Interfaces;
using CollabDoc.Domain.Entities;
using MediatR;

namespace CollabDoc.Application.Documents.Queries.SearchDocuments;

public class SearchDocumentsHandler : IRequestHandler<SearchDocumentsQuery, List<Document>>
{
    private readonly IDocumentRepository _repository;

    public SearchDocumentsHandler(IDocumentRepository repository)
    {
        _repository = repository;
    }

    public async Task<List<Document>> Handle(SearchDocumentsQuery request, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.Keyword))
            throw new ArgumentException("Từ khóa tìm kiếm không được để trống.");

        return await _repository.SearchByTitleAsync(request.Keyword);
    }
}
