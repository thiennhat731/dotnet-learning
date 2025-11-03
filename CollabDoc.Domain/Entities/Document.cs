namespace CollabDoc.Domain.Entities;

public class Document
{
    public string? Id { get; set; }

    public string Title { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    public string OwnerId { get; set; } = string.Empty;

    // Danh sách người được chia sẻ (User IDs hoặc Emails)
    public List<SharedUser> SharedWith { get; set; } = new();

    // Công khai hoặc không (public = ai cũng xem được qua link)
    public bool IsPublic { get; set; } = false;
}
public class SharedUser
{
    public string UserId { get; set; } = string.Empty;
    public DocumentPermission Permission { get; set; } = DocumentPermission.View; // "view" | "edit"
}

