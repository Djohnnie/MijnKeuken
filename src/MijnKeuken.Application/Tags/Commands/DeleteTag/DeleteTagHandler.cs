using MediatR;
using MijnKeuken.Application.Common;
using MijnKeuken.Application.Interfaces;

namespace MijnKeuken.Application.Tags.Commands.DeleteTag;

public class DeleteTagHandler(ITagRepository tagRepository) : IRequestHandler<DeleteTagCommand, Result>
{
    public async Task<Result> Handle(DeleteTagCommand request, CancellationToken cancellationToken)
    {
        var tag = await tagRepository.GetByIdAsync(request.Id, cancellationToken);
        if (tag is null)
            return Result.Failure("Tag niet gevonden.");

        await tagRepository.DeleteAsync(tag, cancellationToken);

        return Result.Success();
    }
}
