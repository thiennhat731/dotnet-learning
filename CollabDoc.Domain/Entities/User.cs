
namespace CollabDoc.Domain.Entities;

public class User
{

    public string Id { get; set; } = string.Empty;

    public string Email { get; set; } = string.Empty;

    public string Password { get; set; } = string.Empty;


    public string? RefreshToken { get; set; } // null khi chưa login


    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}
