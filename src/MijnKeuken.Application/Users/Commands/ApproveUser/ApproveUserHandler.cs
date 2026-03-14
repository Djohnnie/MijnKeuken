using MediatR;
using MijnKeuken.Application.Common;
using MijnKeuken.Application.Interfaces;

namespace MijnKeuken.Application.Users.Commands.ApproveUser;

public class ApproveUserHandler(
    IUserRepository userRepository) : IRequestHandler<ApproveUserCommand, Result>
{
    public async Task<Result> Handle(ApproveUserCommand request, CancellationToken ct)
    {
        var approver = await userRepository.GetByIdAsync(request.ApproverUserId, ct);
        if (approver is null || !approver.IsApproved)
            return Result.Failure("U heeft geen rechten om gebruikers goed te keuren.");

        var target = await userRepository.GetByIdAsync(request.TargetUserId, ct);
        if (target is null)
            return Result.Failure("Gebruiker niet gevonden.");

        if (target.IsApproved)
            return Result.Failure("Gebruiker is al goedgekeurd.");

        target.IsApproved = true;
        await userRepository.UpdateAsync(target, ct);

        return Result.Success();
    }
}
