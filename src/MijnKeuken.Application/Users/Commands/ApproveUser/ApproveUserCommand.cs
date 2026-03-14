using MediatR;
using MijnKeuken.Application.Common;

namespace MijnKeuken.Application.Users.Commands.ApproveUser;

public record ApproveUserCommand(Guid ApproverUserId, Guid TargetUserId) : IRequest<Result>;
