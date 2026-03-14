namespace MijnKeuken.Infrastructure.Services;

public class JwtSettings
{
    public string Secret { get; set; } = string.Empty;
    public string Issuer { get; set; } = "MijnKeuken";
    public string Audience { get; set; } = "MijnKeuken";
    public int ExpirationMinutes { get; set; } = 480;
}
