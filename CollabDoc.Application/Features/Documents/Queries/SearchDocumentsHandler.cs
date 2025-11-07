using CollabDoc.Application.Interfaces;
using CollabDoc.Domain.Entities;
using MediatR;

namespace CollabDoc.Application.Features.Documents.Queries.SearchDocuments;

public class SearchDocumentsHandler : IRequestHandler<SearchDocumentsQuery, List<Document>>
{
    private readonly IDocumentRepository _repository;
    private readonly ICacheService _cacheService;

    public SearchDocumentsHandler(IDocumentRepository repository, ICacheService cacheService)
    {
        _repository = repository;
        _cacheService = cacheService;
    }

    public async Task<List<Document>> Handle(SearchDocumentsQuery request, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.Keyword))
            throw new ArgumentException("Từ khóa tìm kiếm không được để trống.");

        // Tạo cache key cho search
        var cacheKey = $"document_search:{request.Keyword.ToLower().Trim()}";

        // Thử lấy từ cache trước
        var cachedResults = await _cacheService.GetAsync<List<Document>>(cacheKey);
        if (cachedResults != null)
        {
            return cachedResults;
        }

        // Nếu không có trong cache, search từ DB
        var searchResults = await _repository.SearchByTitleAsync(request.Keyword);

        // Cache kết quả với thời gian sống 10 phút
        await _cacheService.SetAsync(cacheKey, searchResults, TimeSpan.FromMinutes(10));

        return searchResults;
    }
}
