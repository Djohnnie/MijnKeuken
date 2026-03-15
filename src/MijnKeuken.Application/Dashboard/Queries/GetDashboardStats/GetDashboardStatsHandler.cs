using MediatR;
using MijnKeuken.Application.Dashboard.DTOs;
using MijnKeuken.Application.Interfaces;

namespace MijnKeuken.Application.Dashboard.Queries.GetDashboardStats;

public class GetDashboardStatsHandler(IMenuEntryRepository menuEntryRepository)
    : IRequestHandler<GetDashboardStatsQuery, DashboardStatsDto>
{
    public async Task<DashboardStatsDto> Handle(GetDashboardStatsQuery request, CancellationToken cancellationToken)
    {
        var topRecipes = await menuEntryRepository.GetTopScheduledRecipesAsync(
            request.TopCount, cancellationToken);

        var topIngredients = await menuEntryRepository.GetTopUsedIngredientsAsync(
            request.TopCount, cancellationToken);

        var nextDelivery = await menuEntryRepository.GetNextDeliveryAsync(cancellationToken);

        return new DashboardStatsDto(topRecipes, topIngredients, nextDelivery);
    }
}
