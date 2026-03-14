using MediatR;
using MijnKeuken.Application.Common;
using MijnKeuken.Application.Interfaces;

namespace MijnKeuken.Application.Users.Commands.UpdateThemePreference;

public class UpdateThemePreferenceHandler(
    IUserRepository userRepository) : IRequestHandler<UpdateThemePreferenceCommand, Result>
{
    public async Task<Result> Handle(UpdateThemePreferenceCommand request, CancellationToken ct)
    {
        var user = await userRepository.GetByIdAsync(request.UserId, ct);
        if (user is null)
            return Result.Failure("Gebruiker niet gevonden.");

        user.PrefersDarkMode = request.PrefersDarkMode;
        await userRepository.UpdateAsync(user, ct);

        return Result.Success();
    }
}
