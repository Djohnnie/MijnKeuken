using MediatR;
using MijnKeuken.Application.Common;

namespace MijnKeuken.Application.Users.Commands.UpdateThemePreference;

public record UpdateThemePreferenceCommand(Guid UserId, bool PrefersDarkMode) : IRequest<Result>;
