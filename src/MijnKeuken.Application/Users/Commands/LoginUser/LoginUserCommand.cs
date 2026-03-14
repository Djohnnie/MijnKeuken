using MediatR;
using MijnKeuken.Application.Common;

namespace MijnKeuken.Application.Users.Commands.LoginUser;

public record LoginUserCommand(string Username, string Password) : IRequest<Result<string>>;
