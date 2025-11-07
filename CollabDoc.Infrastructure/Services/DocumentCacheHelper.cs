using CollabDoc.Application.Interfaces;
using CollabDoc.Domain.Entities;

namespace CollabDoc.Infrastructure.Services;

/// <summary>
/// Helper class để thực hiện các thao tác cache phổ biến cho Document
/// </summary>
public class DocumentCacheHelper
{
    private readonly ICacheService _cacheService;

    public DocumentCacheHelper(ICacheService cacheService)
    {
        _cacheService = cacheService;
    }

    /// <summary>
    /// Xóa tất cả cache liên quan đến một document
    /// </summary>
    public async Task InvalidateDocumentCache(string documentId, string? ownerId = null)
    {
        // Xóa cache document
        await _cacheService.RemoveAsync(CacheKeys.Document(documentId));

        // Nếu có ownerId, xóa cache danh sách documents của owner
        if (!string.IsNullOrEmpty(ownerId))
        {
            await _cacheService.RemoveByPatternAsync($"user_documents:{ownerId}*");
        }

        // Xóa cache search (vì document có thể xuất hiện trong kết quả search)
        await _cacheService.RemoveByPatternAsync("document_search:*");
    }

    /// <summary>
    /// Xóa cache của user (khi user bị thay đổi quyền truy cập)
    /// </summary>
    public async Task InvalidateUserCache(string userId)
    {
        await _cacheService.RemoveByPatternAsync($"user_documents:{userId}*");
        await _cacheService.RemoveByPatternAsync($"shared_documents:{userId}*");
    }

    /// <summary>
    /// Refresh cache cho một document cụ thể
    /// </summary>
    public async Task RefreshDocumentCache(Document document)
    {
        var cacheKey = CacheKeys.Document(document.Id!);
        await _cacheService.SetAsync(cacheKey, document, CacheKeys.DocumentCacheDuration);
    }
}