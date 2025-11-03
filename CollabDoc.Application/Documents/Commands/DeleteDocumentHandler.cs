using CollabDoc.Application.Interfaces;
using MediatR;

namespace CollabDoc.Application.Documents.Commands.DeleteDocument;

public class DeleteDocumentHandler : IRequestHandler<DeleteDocumentCommand>
{
    private readonly IDocumentRepository _repository;

    public DeleteDocumentHandler(IDocumentRepository repository)
    {
        _repository = repository;
    }

    public async Task Handle(DeleteDocumentCommand request, CancellationToken cancellationToken)
    {
        var existing = await _repository.GetByIdAsync(request.Id);
        if (existing is null)
            throw new KeyNotFoundException($"Không tìm thấy tài liệu với ID: {request.Id}");

        await _repository.DeleteAsync(request.Id);
    }
}
