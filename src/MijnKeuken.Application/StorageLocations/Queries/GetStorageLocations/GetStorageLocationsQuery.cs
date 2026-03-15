using MediatR;
using MijnKeuken.Application.StorageLocations.DTOs;

namespace MijnKeuken.Application.StorageLocations.Queries.GetStorageLocations;

public record GetStorageLocationsQuery : IRequest<List<StorageLocationDto>>;
