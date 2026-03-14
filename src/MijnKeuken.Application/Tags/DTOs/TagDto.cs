using MijnKeuken.Domain.Entities;

namespace MijnKeuken.Application.Tags.DTOs;

public record TagDto(Guid Id, string Name, TagType Type, string Color);
