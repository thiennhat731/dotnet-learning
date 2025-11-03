using CollabDoc.Application.Interfaces;
using CollabDoc.Domain.Entities;
using MediatR;

namespace CollabDoc.Application.Documents.Commands.CreateDocument;

public class CreateDocumentHandler : IRequestHandler<CreateDocumentCommand, Document>
{
    private readonly IDocumentRepository _repository;

    public CreateDocumentHandler(IDocumentRepository repository)
    {
        _repository = repository;
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
        return doc;
    }
}
