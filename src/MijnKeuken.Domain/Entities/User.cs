namespace MijnKeuken.Domain.Entities;

public class User : BaseEntity
{
    public string Username { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public bool IsApproved { get; set; }
    public bool PrefersDarkMode { get; set; }
    public DateTime CreatedAt { get; set; }
}
