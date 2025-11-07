using CollabDoc.Application.Interfaces;

namespace CollabDoc.Infrastructure.Services;

/// <summary>
/// Extension methods để đơn giản hóa các thao tác cache phổ biến cho Document
/// </summary>
public static class DocumentCacheExtensions
{
    /// <summary>
    /// Invalidate tất cả cache liên quan đến document
    /// </summary>
    public static async Task InvalidateDocumentCacheAsync(this ICacheService cacheService, string documentId, string? ownerId = null)
    {
        // Xóa cache document
        await cacheService.RemoveAsync($"document:{documentId}");

        // Nếu có ownerId, xóa cache danh sách documents của owner
        if (!string.IsNullOrEmpty(ownerId))
        {
            await cacheService.RemoveByPatternAsync($"user_documents:{ownerId}*");
        }

        // Xóa cache search vì có thay đổi
        await cacheService.RemoveByPatternAsync("document_search:*");
    }

    /// <summary>
    /// Invalidate cache của user
    /// </summary>
    public static async Task InvalidateUserDocumentsCacheAsync(this ICacheService cacheService, string userId)
    {
        await cacheService.RemoveByPatternAsync($"user_documents:{userId}*");
    }

    /// <summary>
    /// Invalidate tất cả search cache
    /// </summary>
    public static async Task InvalidateSearchCacheAsync(this ICacheService cacheService)
    {
        await cacheService.RemoveByPatternAsync("document_search:*");
    }
}