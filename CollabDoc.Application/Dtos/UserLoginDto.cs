using CollabDoc.Domain.Entities;

namespace CollabDoc.Application.Dtos;

public class UserLoginDto
{
    public string Email { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
}
public class ResLoginDto
{
    public User User { get; set; } = new User();
    public string AccessToken { get; set; } = string.Empty;
    public string RefreshToken { get; set; } = string.Empty;
}