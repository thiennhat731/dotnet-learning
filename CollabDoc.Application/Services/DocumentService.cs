using CollabDoc.Domain.Entities;
using CollabDoc.Application.Interfaces;
using CollabDoc.Application.Dtos;
using Microsoft.AspNetCore.Http;
namespace CollabDoc.Application.Services;

public class DocumentService
{
    private readonly IDocumentRepository _repository;
    private readonly IHttpContextAccessor _httpContextAccessor;

    public DocumentService(IDocumentRepository repository, IHttpContextAccessor httpContextAccessor)
    {
        _repository = repository;
        _httpContextAccessor = httpContextAccessor;
    }

    public async Task<PaginationResponse<Document>> GetAllAsync(
     int page = 1,
     int pageSize = 10,
     string sortBy = "CreatedAt",
     bool isDescending = true,
     string? keyword = null,
     string? userId = null)
    {
        if (string.IsNullOrEmpty(userId))
            throw new UnauthorizedAccessException("User not authenticated.");

        var total = await _repository.CountAsync(keyword, userId);
        var skip = (page - 1) * pageSize;

        var items = await _repository.GetPagedAsync(skip, pageSize, sortBy, isDescending, keyword, userId);
        return new PaginationResponse<Document>(items, page, pageSize, total);
    }



    public async Task<Document> GetByIdAsync(string id)
    {
        // Validate ID định dạng ObjectId

        var doc = await _repository.GetByIdAsync(id);
        if (doc is null)
            throw new KeyNotFoundException($"Không tìm thấy tài liệu với ID: {id}");

        return doc;
    }

    public async Task<Document> CreateAsync(DocumentCreateDto dto, string ownerId)
    {
        // Validate cơ bản
        if (string.IsNullOrWhiteSpace(dto.Title))
            throw new ArgumentException("Tiêu đề không được để trống.");

        if (dto.Title.Length < 3)
            throw new ArgumentException("Tiêu đề phải có ít nhất 3 ký tự.");

        var doc = new Document
        {
            Title = dto.Title,
            Content = dto.Content,
            OwnerId = ownerId
        };

        await _repository.CreateAsync(doc);
        return doc;
    }

    public async Task UpdateAsync(string id, Document doc)
    {

        var existing = await _repository.GetByIdAsync(id);
        if (existing is null)
            throw new KeyNotFoundException($"Không tìm thấy tài liệu với ID: {id}");

        if (existing.Title == doc.Title)
            throw new InvalidOperationException("Tiêu đề trùng với tài liệu hiện có.");

        await _repository.UpdateAsync(id, doc);
    }

    public async Task DeleteAsync(string id)
    {
        var existing = await _repository.GetByIdAsync(id);
        if (existing is null)
            throw new KeyNotFoundException($"Không tìm thấy tài liệu với ID: {id}");

        await _repository.DeleteAsync(id);
    }
    public async Task<List<Document>> SearchByTitleAsync(string keyword)
    {
        if (string.IsNullOrWhiteSpace(keyword))
            throw new ArgumentException("Từ khóa tìm kiếm không được để trống.");

        return await _repository.SearchByTitleAsync(keyword);
    }
}
