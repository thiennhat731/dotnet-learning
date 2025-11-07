using CollabDoc.Application.Interfaces;
using CollabDoc.Domain.Entities;
using MediatR;

namespace CollabDoc.Application.Features.Documents.Commands.CreateDocument;

public class CreateDocumentHandler : IRequestHandler<CreateDocumentCommand, Document>
{
    private readonly IDocumentRepository _repository;
    private readonly ICacheService _cacheService;

    public CreateDocumentHandler(IDocumentRepository repository, ICacheService cacheService)
    {
        _repository = repository;
        _cacheService = cacheService;
    }

    public async Task<Document> Handle(CreateDocumentCommand request, CancellationToken cancellationToken)
    {
        var dto = request.Dto;

        if (string.IsNullOrWhiteSpace(dto.Title))
            throw new ArgumentException("Tiêu đề không được để trống.");
        if (dto.Title.Length < 3)
            throw new ArgumentException("Tiêu đề phải có ít nhất 3 ký tự.");

        var doc = new Document
        {
            Title = dto.Title,
            Content = dto.Content ?? "",
            OwnerId = request.OwnerId
        };

        await _repository.CreateAsync(doc);

        // Cache document mới tạo
        var cacheKey = $"document:{doc.Id}";
        await _cacheService.SetAsync(cacheKey, doc, TimeSpan.FromMinutes(30));

        // Invalidate cache danh sách documents của user
        await _cacheService.RemoveByPatternAsync($"user_documents:{request.OwnerId}*");

        // Invalidate search cache vì có document mới
        await _cacheService.RemoveByPatternAsync("document_search:*");

        return doc;
    }
}
