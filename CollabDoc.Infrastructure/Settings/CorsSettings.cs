using System.ComponentModel.DataAnnotations;

namespace CollabDoc.Infrastructure.Settings;

public class CorsSettings
{
    public const string SectionName = "CorsSettings";

    [Required]
    public string[] AllowedOrigins { get; set; } = Array.Empty<string>();

    public string[] AllowedMethods { get; set; } = { "GET", "POST", "PUT", "DELETE", "OPTIONS" };

    public string[] AllowedHeaders { get; set; } = Array.Empty<string>();

    public bool AllowCredentials { get; set; } = true;
}