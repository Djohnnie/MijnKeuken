using MediatR;
using MijnKeuken.Application.Common;

namespace MijnKeuken.Application.Users.Commands.RegisterUser;

public record RegisterUserCommand(string Username, string Password, string Email) : IRequest<Result>;
