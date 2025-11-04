namespace day11.Infrastructure.Jwt;

public class JwtSettings
{
    public string Key { get; set; } = "super-secret-key-123456789";
    public string Issuer { get; set; } = "collabdoc-api";
    public string Audience { get; set; } = "collabdoc-client";
    public int ExpiryMinutes { get; set; } = 30;
}
