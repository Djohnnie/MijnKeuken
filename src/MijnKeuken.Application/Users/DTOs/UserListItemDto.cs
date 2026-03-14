namespace MijnKeuken.Application.Users.DTOs;

public record UserListItemDto(
    Guid Id,
    string Username,
    string Email,
    bool IsApproved,
    DateTime CreatedAt);
