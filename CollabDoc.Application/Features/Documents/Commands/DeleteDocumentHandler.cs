using CollabDoc.Application.Interfaces;
using MediatR;

namespace CollabDoc.Application.Features.Documents.Commands.DeleteDocument;

public class DeleteDocumentHandler : IRequestHandler<DeleteDocumentCommand>
{
    private readonly IDocumentRepository _repository;
    private readonly ICacheService _cacheService;

    public DeleteDocumentHandler(IDocumentRepository repository, ICacheService cacheService)
    {
        _repository = repository;
        _cacheService = cacheService;
    }

    public async Task Handle(DeleteDocumentCommand request, CancellationToken cancellationToken)
    {
        var existing = await _repository.GetByIdAsync(request.Id);
        if (existing is null)
            throw new KeyNotFoundException($"Không tìm thấy tài liệu với ID: {request.Id}");

        await _repository.DeleteAsync(request.Id);

        // Invalidate cache của document này
        await _cacheService.RemoveAsync($"document:{request.Id}");

        // Invalidate cache danh sách documents của owner
        await _cacheService.RemoveByPatternAsync($"user_documents:{existing.OwnerId}*");

        // Invalidate search cache
        await _cacheService.RemoveByPatternAsync("document_search:*");
    }
}
