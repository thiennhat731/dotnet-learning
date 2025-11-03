using CollabDoc.Domain.Entities;

namespace CollabDoc.Application.Interfaces;

public interface IDocumentRepository
{
    Task<List<Document>> GetAllAsync();
    Task<Document?> GetByIdAsync(string id);
    Task CreateAsync(Document doc);
    Task UpdateAsync(string id, Document doc);
    Task DeleteAsync(string id);
    Task<List<Document>> SearchByTitleAsync(string keyword);
    Task<List<Document>> GetPagedAsync(int skip, int limit, string sortBy, bool desc, string? keyword = null, string? userId = null);
    Task<long> CountAsync(string? keyword = null, string? userId = null);
    Task UpdateContentAsync(string id, string content);
}
