using MediatR;
using MijnKeuken.Application.Common;

namespace MijnKeuken.Application.StorageLocations.Commands.UpdateStorageLocation;

public record UpdateStorageLocationCommand(Guid Id, string Name, string Description) : IRequest<Result>;
