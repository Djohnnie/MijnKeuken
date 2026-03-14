using MediatR;
using MijnKeuken.Application.Common;
using MijnKeuken.Application.Interfaces;
using MijnKeuken.Application.Users.DTOs;

namespace MijnKeuken.Application.Users.Queries.GetUserProfile;

public class GetUserProfileHandler(
    IUserRepository userRepository) : IRequestHandler<GetUserProfileQuery, Result<UserProfileDto>>
{
    public async Task<Result<UserProfileDto>> Handle(GetUserProfileQuery request, CancellationToken ct)
    {
        var user = await userRepository.GetByIdAsync(request.UserId, ct);
        if (user is null)
            return Result<UserProfileDto>.Failure("Gebruiker niet gevonden.");

        var dto = new UserProfileDto(
            user.Id,
            user.Username,
            user.Email,
            user.IsApproved,
            user.CreatedAt);

        return Result<UserProfileDto>.Success(dto);
    }
}
