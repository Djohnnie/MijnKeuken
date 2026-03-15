using MediatR;
using MijnKeuken.Application.Common;

namespace MijnKeuken.Application.Menu.Commands.DeleteMenuEntry;

public record DeleteMenuEntryCommand(Guid Id) : IRequest<Result>;
