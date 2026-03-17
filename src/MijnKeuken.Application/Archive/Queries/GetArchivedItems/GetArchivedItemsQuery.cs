using MediatR;
using MijnKeuken.Application.Archive.DTOs;

namespace MijnKeuken.Application.Archive.Queries.GetArchivedItems;

public record GetArchivedItemsQuery : IRequest<List<ArchivedItemDto>>;
