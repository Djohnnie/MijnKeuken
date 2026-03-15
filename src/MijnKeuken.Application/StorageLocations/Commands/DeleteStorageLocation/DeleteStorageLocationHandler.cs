using MediatR;
using MijnKeuken.Application.Common;
using MijnKeuken.Application.Interfaces;

namespace MijnKeuken.Application.StorageLocations.Commands.DeleteStorageLocation;

public class DeleteStorageLocationHandler(IStorageLocationRepository repository)
    : IRequestHandler<DeleteStorageLocationCommand, Result>
{
    public async Task<Result> Handle(DeleteStorageLocationCommand request, CancellationToken cancellationToken)
    {
        var location = await repository.GetByIdAsync(request.Id, cancellationToken);
        if (location is null)
            return Result.Failure("Opslaglocatie niet gevonden.");

        await repository.DeleteAsync(location, cancellationToken);

        return Result.Success();
    }
}
