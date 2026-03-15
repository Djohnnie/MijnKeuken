using MediatR;
using MijnKeuken.Application.Common;
using MijnKeuken.Application.Interfaces;

namespace MijnKeuken.Application.StorageLocations.Commands.UpdateStorageLocation;

public class UpdateStorageLocationHandler(IStorageLocationRepository repository)
    : IRequestHandler<UpdateStorageLocationCommand, Result>
{
    public async Task<Result> Handle(UpdateStorageLocationCommand request, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.Name))
            return Result.Failure("Naam is verplicht.");

        var location = await repository.GetByIdAsync(request.Id, cancellationToken);
        if (location is null)
            return Result.Failure("Opslaglocatie niet gevonden.");

        location.Name = request.Name.Trim();
        location.Description = request.Description?.Trim() ?? string.Empty;

        await repository.UpdateAsync(location, cancellationToken);

        return Result.Success();
    }
}
