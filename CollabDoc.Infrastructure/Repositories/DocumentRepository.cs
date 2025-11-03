using CollabDoc.Domain.Entities;
using CollabDoc.Infrastructure.Settings;
using MongoDB.Driver;
using CollabDoc.Application.Interfaces;

namespace CollabDoc.Infrastructure.Repositories;

public class DocumentRepository : IDocumentRepository
{
    private readonly IMongoCollection<Document> _collection;

    public DocumentRepository(MongoDbSettings settings)
    {
        var client = new MongoClient(settings.ConnectionString);
        var db = client.GetDatabase(settings.DatabaseName);
        _collection = db.GetCollection<Document>(settings.DocumentsCollection);

        // Tạo index full-text cho Title (phục vụ search)
        var indexKeys = Builders<Document>.IndexKeys.Text(d => d.Title);
        var indexModel = new CreateIndexModel<Document>(indexKeys);
        _collection.Indexes.CreateOne(indexModel);
    }

    // Lấy toàn bộ tài liệu
    public async Task<List<Document>> GetAllAsync()
        => await _collection.Find(_ => true).ToListAsync();

    // Tích hợp Search + Sort + Pagination
    // Tích hợp Search + Sort + Pagination + Filter theo OwnerId
    public async Task<List<Document>> GetPagedAsync(
        int skip,
        int limit,
        string sortBy,
        bool desc,
        string? keyword = null,
        string? userId = null)
    {
        var filters = new List<FilterDefinition<Document>>();

        // 🔹 Lọc theo OwnerId nếu có
        if (!string.IsNullOrEmpty(userId))
            filters.Add(Builders<Document>.Filter.Eq(d => d.OwnerId, userId));

        // 🔹 Thêm keyword nếu có
        if (!string.IsNullOrWhiteSpace(keyword))
            filters.Add(Builders<Document>.Filter.Text(keyword));

        var filter = filters.Any()
            ? Builders<Document>.Filter.And(filters)
            : Builders<Document>.Filter.Empty;

        // Sort
        var property = typeof(Document).GetProperty(sortBy,
            System.Reflection.BindingFlags.IgnoreCase |
            System.Reflection.BindingFlags.Public |
            System.Reflection.BindingFlags.Instance);

        var validSortBy = property != null ? property.Name : nameof(Document.CreatedAt);

        SortDefinition<Document> sort = desc
            ? Builders<Document>.Sort.Descending(validSortBy)
            : Builders<Document>.Sort.Ascending(validSortBy);

        return await _collection
            .Find(filter)
            .Sort(sort)
            .Skip(skip)
            .Limit(limit)
            .ToListAsync();
    }

    // Đếm tổng số tài liệu có filter userId + keyword
    public async Task<long> CountAsync(string? keyword = null, string? userId = null)
    {
        var filters = new List<FilterDefinition<Document>>();

        if (!string.IsNullOrEmpty(userId))
            filters.Add(Builders<Document>.Filter.Eq(d => d.OwnerId, userId));

        if (!string.IsNullOrWhiteSpace(keyword))
            filters.Add(Builders<Document>.Filter.Text(keyword));

        var filter = filters.Any()
            ? Builders<Document>.Filter.And(filters)
            : Builders<Document>.Filter.Empty;

        return await _collection.CountDocumentsAsync(filter);
    }

    // Đếm tổng số tài liệu (có hỗ trợ filter theo keyword)
    public async Task<long> CountAsync(string? keyword = null)
    {
        var filter = string.IsNullOrWhiteSpace(keyword)
            ? Builders<Document>.Filter.Empty
            : Builders<Document>.Filter.Text(keyword);

        return await _collection.CountDocumentsAsync(filter);
    }


    // Đếm tất cả tài liệu (không filter)
    public async Task<long> CountAsync()
        => await _collection.CountDocumentsAsync(_ => true);

    // Tìm tài liệu theo ID
    public async Task<Document?> GetByIdAsync(string id)
        => await _collection.Find(x => x.Id == id).FirstOrDefaultAsync();

    // Thêm mới tài liệu
    public async Task CreateAsync(Document doc)
    {
        doc.CreatedAt = DateTime.UtcNow;
        doc.UpdatedAt = DateTime.UtcNow;
        await _collection.InsertOneAsync(doc);
    }

    //  Update toàn bộ tài liệu
    public async Task UpdateAsync(string id, Document doc)
    {
        doc.UpdatedAt = DateTime.UtcNow;
        doc.Id = id;

        await _collection.ReplaceOneAsync(x => x.Id == id, doc);
    }

    // Xóa tài liệu
    public async Task DeleteAsync(string id)
        => await _collection.DeleteOneAsync(x => x.Id == id);

    // Search riêng (nếu cần dùng độc lập)
    public async Task<List<Document>> SearchByTitleAsync(string keyword)
    {
        if (string.IsNullOrWhiteSpace(keyword))
            throw new ArgumentException("Từ khóa tìm kiếm không được để trống.", nameof(keyword));

        var filter = Builders<Document>.Filter.Text(keyword);
        return await _collection.Find(filter).ToListAsync();
    }
    public async Task UpdateContentAsync(string id, string base64Content)
    {
        var doc = await _collection.Find(d => d.Id == id).FirstOrDefaultAsync();
        if (doc == null) return;

        doc.Content = base64Content;
        doc.UpdatedAt = DateTime.UtcNow;
        await _collection.ReplaceOneAsync(d => d.Id == id, doc);
    }
}
