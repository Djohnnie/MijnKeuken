using MediatR;
using MijnKeuken.Application.Common;
using MijnKeuken.Application.Users.DTOs;

namespace MijnKeuken.Application.Users.Queries.GetUserProfile;

public record GetUserProfileQuery(Guid UserId) : IRequest<Result<UserProfileDto>>;
