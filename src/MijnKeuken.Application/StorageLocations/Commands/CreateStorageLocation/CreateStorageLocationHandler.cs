using MediatR;
using MijnKeuken.Application.Common;
using MijnKeuken.Application.Interfaces;
using MijnKeuken.Domain.Entities;

namespace MijnKeuken.Application.StorageLocations.Commands.CreateStorageLocation;

public class CreateStorageLocationHandler(IStorageLocationRepository repository)
    : IRequestHandler<CreateStorageLocationCommand, Result<Guid>>
{
    public async Task<Result<Guid>> Handle(CreateStorageLocationCommand request, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.Name))
            return Result<Guid>.Failure("Naam is verplicht.");

        if (await repository.ExistsByNameAsync(request.Name.Trim(), cancellationToken))
            return Result<Guid>.Failure("Er bestaat al een opslaglocatie met deze naam.");

        var location = new StorageLocation
        {
            Id = Guid.NewGuid(),
            Name = request.Name.Trim(),
            Description = request.Description?.Trim() ?? string.Empty
        };

        await repository.AddAsync(location, cancellationToken);

        return Result<Guid>.Success(location.Id);
    }
}
