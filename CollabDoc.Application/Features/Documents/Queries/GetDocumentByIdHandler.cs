using CollabDoc.Application.Interfaces;
using CollabDoc.Domain.Entities;
using MediatR;

namespace CollabDoc.Application.Features.Documents.Queries.GetDocumentById;

public class GetDocumentByIdHandler : IRequestHandler<GetDocumentByIdQuery, Document>
{
    private readonly IDocumentRepository _repository;
    private readonly ICacheService _cacheService;

    public GetDocumentByIdHandler(IDocumentRepository repository, ICacheService cacheService)
    {
        _repository = repository;
        _cacheService = cacheService;
    }

    public async Task<Document> Handle(GetDocumentByIdQuery request, CancellationToken cancellationToken)
    {
        // Thử lấy từ cache trước
        var cacheKey = $"document:{request.Id}";
        var cachedDoc = await _cacheService.GetAsync<Document>(cacheKey);

        if (cachedDoc != null)
        {
            return cachedDoc;
        }

        // Nếu không có trong cache, lấy từ DB
        var doc = await _repository.GetByIdAsync(request.Id);
        if (doc is null)
            throw new KeyNotFoundException($"Không tìm thấy tài liệu với ID: {request.Id}");

        // Cache document với thời gian sống 30 phút
        await _cacheService.SetAsync(cacheKey, doc, TimeSpan.FromMinutes(30));

        return doc;
    }
}
