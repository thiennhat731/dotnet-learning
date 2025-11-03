namespace CollabDoc.Application.Dtos;

public class DocumentMetadataDto
{
    public string? Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string OwnerId { get; set; } = string.Empty;
    public bool IsPublic { get; set; } = false;
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public int SharedCount { get; set; } // số người được chia sẻ
}
