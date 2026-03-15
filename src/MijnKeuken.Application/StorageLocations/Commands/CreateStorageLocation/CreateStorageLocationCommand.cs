using MediatR;
using MijnKeuken.Application.Common;

namespace MijnKeuken.Application.StorageLocations.Commands.CreateStorageLocation;

public record CreateStorageLocationCommand(string Name, string Description) : IRequest<Result<Guid>>;
