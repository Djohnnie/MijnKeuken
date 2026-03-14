namespace MijnKeuken.Application.Users.DTOs;

public record UserProfileDto(
    Guid Id,
    string Username,
    string Email,
    bool IsApproved,
    DateTime CreatedAt);
