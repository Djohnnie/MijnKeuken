using MediatR;
using MijnKeuken.Application.Common;
using MijnKeuken.Application.Interfaces;
using MijnKeuken.Domain.Entities;

namespace MijnKeuken.Application.Tags.Commands.CreateTag;

public class CreateTagHandler(ITagRepository tagRepository) : IRequestHandler<CreateTagCommand, Result<Guid>>
{
    public async Task<Result<Guid>> Handle(CreateTagCommand request, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.Name))
            return Result<Guid>.Failure("Naam is verplicht.");

        if (await tagRepository.ExistsByNameAndTypeAsync(request.Name.Trim(), request.Type, cancellationToken))
            return Result<Guid>.Failure("Er bestaat al een tag met deze naam en type.");

        var tag = new Tag
        {
            Id = Guid.NewGuid(),
            Name = request.Name.Trim(),
            Type = request.Type,
            Color = request.Color
        };

        await tagRepository.AddAsync(tag, cancellationToken);

        return Result<Guid>.Success(tag.Id);
    }
}
