using MediatR;
using MijnKeuken.Application.Interfaces;
using MijnKeuken.Application.Menu.DTOs;

namespace MijnKeuken.Application.Menu.Queries.GetMenuEntries;

public class GetMenuEntriesHandler(IMenuEntryRepository repository)
    : IRequestHandler<GetMenuEntriesQuery, List<MenuEntryDto>>
{
    public async Task<List<MenuEntryDto>> Handle(GetMenuEntriesQuery request, CancellationToken cancellationToken)
    {
        var entries = await repository.GetByDateRangeAsync(request.From, request.To, cancellationToken);
        return entries.Select(e => new MenuEntryDto(
            e.Id,
            e.Date,
            e.RecipeId,
            e.Recipe?.Title,
            e.HasDelivery,
            e.DeliveryNote,
            e.IsConsumed,
            e.IsEatingOut
        )).ToList();
    }
}
