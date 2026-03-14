using MediatR;
using MijnKeuken.Application.Tags.DTOs;

namespace MijnKeuken.Application.Tags.Queries.GetTags;

public record GetTagsQuery : IRequest<List<TagDto>>;
