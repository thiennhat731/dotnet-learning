namespace CollabDoc.Infrastructure.Services;

public static class CacheKeys
{
    // Document cache keys
    public static string Document(string documentId) => $"document:{documentId}";
    public static string UserDocuments(string userId) => $"user_documents:{userId}";
    public static string DocumentSearch(string query) => $"document_search:{query}";

    // Cache TTL (Time To Live) - thời gian cache tồn tại
    public static readonly TimeSpan DocumentCacheDuration = TimeSpan.FromMinutes(30);
    public static readonly TimeSpan UserDocumentsCacheDuration = TimeSpan.FromMinutes(15);
    public static readonly TimeSpan SearchCacheDuration = TimeSpan.FromMinutes(10);
}
