using CollabDoc.Application.Interfaces;
using CollabDoc.Domain.Entities;
using MediatR;

namespace CollabDoc.Application.Documents.Queries.GetDocumentById;

public class GetDocumentByIdHandler : IRequestHandler<GetDocumentByIdQuery, Document>
{
    private readonly IDocumentRepository _repository;

    public GetDocumentByIdHandler(IDocumentRepository repository)
    {
        _repository = repository;
    }

    public async Task<Document> Handle(GetDocumentByIdQuery request, CancellationToken cancellationToken)
    {
        var doc = await _repository.GetByIdAsync(request.Id);
        if (doc is null)
            throw new KeyNotFoundException($"Không tìm thấy tài liệu với ID: {request.Id}");
        return doc;
    }
}
