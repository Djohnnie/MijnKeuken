using MediatR;
using MijnKeuken.Application.Common;

namespace MijnKeuken.Application.Tags.Commands.DeleteTag;

public record DeleteTagCommand(Guid Id) : IRequest<Result>;
