using MijnKeuken.Application.Common;
using MijnKeuken.Application.Users.DTOs;

namespace MijnKeuken.Web.Services;

public interface IUserService
{
    Task<Result<UserProfileDto>> GetProfileAsync();
    Task<List<UserListItemDto>> GetAllUsersAsync();
    Task<List<PendingUserDto>> GetPendingUsersAsync();
    Task<Result> ApproveUserAsync(Guid targetUserId);
    Task<Result> UpdateThemePreferenceAsync(bool prefersDarkMode);
}
