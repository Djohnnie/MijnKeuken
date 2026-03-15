using MediatR;
using MijnKeuken.Application.Common;

namespace MijnKeuken.Application.StorageLocations.Commands.DeleteStorageLocation;

public record DeleteStorageLocationCommand(Guid Id) : IRequest<Result>;
