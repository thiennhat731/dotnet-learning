using CollabDoc.Application.Dtos;
using CollabDoc.Application.Interfaces;
using CollabDoc.Domain.Entities;
using MediatR;

namespace CollabDoc.Application.Features.Documents.Queries.GetAllDocuments;

public class GetAllDocumentsHandler : IRequestHandler<GetAllDocumentsQuery, PaginationResponse<Document>>
{
    private readonly IDocumentRepository _repository;
    private readonly ICacheService _cacheService;

    public GetAllDocumentsHandler(IDocumentRepository repository, ICacheService cacheService)
    {
        _repository = repository;
        _cacheService = cacheService;
    }

    public async Task<PaginationResponse<Document>> Handle(GetAllDocumentsQuery request, CancellationToken cancellationToken)
    {
        if (string.IsNullOrEmpty(request.UserId))
            throw new UnauthorizedAccessException("User not authenticated.");

        // Tạo cache key dựa trên parameters
        var cacheKey = $"user_documents:{request.UserId}_{request.Page}_{request.PageSize}_{request.SortBy}_{request.IsDescending}_{request.Keyword ?? ""}";

        // Thử lấy từ cache trước
        var cachedResult = await _cacheService.GetAsync<PaginationResponse<Document>>(cacheKey);
        if (cachedResult != null)
        {
            return cachedResult;
        }

        // Nếu không có trong cache, lấy từ DB
        var total = await _repository.CountAsync(request.Keyword, request.UserId);
        var skip = (request.Page - 1) * request.PageSize;

        var items = await _repository.GetPagedAsync(skip, request.PageSize, request.SortBy, request.IsDescending, request.Keyword, request.UserId);
        var result = new PaginationResponse<Document>(items, request.Page, request.PageSize, total);

        // Cache kết quả với thời gian sống 15 phút
        await _cacheService.SetAsync(cacheKey, result, TimeSpan.FromMinutes(15));

        return result;
    }
}
