using MediatR;
using MijnKeuken.Application.Common;
using MijnKeuken.Domain.Entities;

namespace MijnKeuken.Application.Tags.Commands.CreateTag;

public record CreateTagCommand(string Name, TagType Type, string Color) : IRequest<Result<Guid>>;
