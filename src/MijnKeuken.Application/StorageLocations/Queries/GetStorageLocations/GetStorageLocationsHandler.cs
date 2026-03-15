using MediatR;
using MijnKeuken.Application.Interfaces;
using MijnKeuken.Application.StorageLocations.DTOs;

namespace MijnKeuken.Application.StorageLocations.Queries.GetStorageLocations;

public class GetStorageLocationsHandler(IStorageLocationRepository repository)
    : IRequestHandler<GetStorageLocationsQuery, List<StorageLocationDto>>
{
    public async Task<List<StorageLocationDto>> Handle(GetStorageLocationsQuery request, CancellationToken cancellationToken)
    {
        var locations = await repository.GetAllAsync(cancellationToken);
        return locations.Select(l => new StorageLocationDto(l.Id, l.Name, l.Description)).ToList();
    }
}
