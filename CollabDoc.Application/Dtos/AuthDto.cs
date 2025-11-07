using CollabDoc.Application.Dtos;
using CollabDoc.Domain.Entities;
namespace CollabDoc.Application.Dtos.Auth;

public class UserLoginDto
{
    public string Email { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
}
public class ResLoginDto
{
    public UserResponseDto User { get; set; } = new UserResponseDto();
    public string AccessToken { get; set; } = string.Empty;
    public string RefreshToken { get; set; } = string.Empty;
}
public class UserRegisterDto
{
    public string Email { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
}
