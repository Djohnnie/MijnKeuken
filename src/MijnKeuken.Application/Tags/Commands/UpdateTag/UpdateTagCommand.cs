using MediatR;
using MijnKeuken.Application.Common;
using MijnKeuken.Domain.Entities;

namespace MijnKeuken.Application.Tags.Commands.UpdateTag;

public record UpdateTagCommand(Guid Id, string Name, TagType Type, string Color) : IRequest<Result>;
