namespace MijnKeuken.Application.Users.DTOs;

public record PendingUserDto(Guid Id, string Username, string Email, DateTime CreatedAt);
