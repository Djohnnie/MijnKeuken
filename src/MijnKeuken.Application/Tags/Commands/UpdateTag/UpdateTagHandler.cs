using MediatR;
using MijnKeuken.Application.Common;
using MijnKeuken.Application.Interfaces;

namespace MijnKeuken.Application.Tags.Commands.UpdateTag;

public class UpdateTagHandler(ITagRepository tagRepository) : IRequestHandler<UpdateTagCommand, Result>
{
    public async Task<Result> Handle(UpdateTagCommand request, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.Name))
            return Result.Failure("Naam is verplicht.");

        var tag = await tagRepository.GetByIdAsync(request.Id, cancellationToken);
        if (tag is null)
            return Result.Failure("Tag niet gevonden.");

        tag.Name = request.Name.Trim();
        tag.Type = request.Type;
        tag.Color = request.Color;

        await tagRepository.UpdateAsync(tag, cancellationToken);

        return Result.Success();
    }
}
