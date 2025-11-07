using CollabDoc.Application.Interfaces;
using MediatR;

namespace CollabDoc.Application.Features.Documents.Commands.UpdateDocument;

public class UpdateDocumentHandler : IRequestHandler<UpdateDocumentCommand>
{
    private readonly IDocumentRepository _repository;
    private readonly ICacheService _cacheService;

    public UpdateDocumentHandler(IDocumentRepository repository, ICacheService cacheService)
    {
        _repository = repository;
        _cacheService = cacheService;
    }

    public async Task Handle(UpdateDocumentCommand request, CancellationToken cancellationToken)
    {
        var existing = await _repository.GetByIdAsync(request.Id);
        if (existing is null)
            throw new KeyNotFoundException($"Không tìm thấy tài liệu với ID: {request.Id}");

        if (existing.Title == request.Document.Title)
            throw new InvalidOperationException("Tiêu đề trùng với tài liệu hiện có.");

        await _repository.UpdateAsync(request.Id, request.Document);

        // Invalidate cache của document này
        await _cacheService.RemoveAsync($"document:{request.Id}");

        // Invalidate cache danh sách documents của owner
        await _cacheService.RemoveByPatternAsync($"user_documents:{existing.OwnerId}*");

        // Invalidate search cache vì có thay đổi
        await _cacheService.RemoveByPatternAsync("document_search:*");
    }
}
