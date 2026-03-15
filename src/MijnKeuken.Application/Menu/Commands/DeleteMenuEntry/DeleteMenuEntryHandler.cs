using MediatR;
using MijnKeuken.Application.Common;
using MijnKeuken.Application.Interfaces;

namespace MijnKeuken.Application.Menu.Commands.DeleteMenuEntry;

public class DeleteMenuEntryHandler(IMenuEntryRepository repository)
    : IRequestHandler<DeleteMenuEntryCommand, Result>
{
    public async Task<Result> Handle(DeleteMenuEntryCommand request, CancellationToken cancellationToken)
    {
        var entry = await repository.GetByIdAsync(request.Id, cancellationToken);
        if (entry is null)
            return Result.Failure("Menu-item niet gevonden.");

        await repository.DeleteAsync(entry, cancellationToken);
        return Result.Success();
    }
}
