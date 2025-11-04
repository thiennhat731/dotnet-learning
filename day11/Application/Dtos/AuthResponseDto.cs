namespace day11.Application.Dtos;

public record AuthResponseDto(string UserId, string Email, string AccessToken, string RefreshToken);
