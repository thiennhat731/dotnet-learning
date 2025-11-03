using CollabDoc.Application.Interfaces;
using MediatR;

namespace CollabDoc.Application.Documents.Commands.UpdateDocument;

public class UpdateDocumentHandler : IRequestHandler<UpdateDocumentCommand>
{
    private readonly IDocumentRepository _repository;

    public UpdateDocumentHandler(IDocumentRepository repository)
    {
        _repository = repository;
    }

    public async Task Handle(UpdateDocumentCommand request, CancellationToken cancellationToken)
    {
        var existing = await _repository.GetByIdAsync(request.Id);
        if (existing is null)
            throw new KeyNotFoundException($"Không tìm thấy tài liệu với ID: {request.Id}");

        if (existing.Title == request.Document.Title)
            throw new InvalidOperationException("Tiêu đề trùng với tài liệu hiện có.");

        await _repository.UpdateAsync(request.Id, request.Document);
    }
}
