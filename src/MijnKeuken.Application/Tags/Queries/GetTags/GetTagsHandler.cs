using MediatR;
using MijnKeuken.Application.Interfaces;
using MijnKeuken.Application.Tags.DTOs;

namespace MijnKeuken.Application.Tags.Queries.GetTags;

public class GetTagsHandler(ITagRepository tagRepository) : IRequestHandler<GetTagsQuery, List<TagDto>>
{
    public async Task<List<TagDto>> Handle(GetTagsQuery request, CancellationToken cancellationToken)
    {
        var tags = await tagRepository.GetAllAsync(cancellationToken);
        return tags.Select(t => new TagDto(t.Id, t.Name, t.Type, t.Color)).ToList();
    }
}
