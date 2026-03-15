using MijnKeuken.Application.Common;
using MijnKeuken.Application.Menu.DTOs;

namespace MijnKeuken.Web.Services;

public interface IMenuService
{
    Task<List<MenuEntryDto>> GetByRangeAsync(DateOnly from, DateOnly to);
    Task<Result<Guid>> UpsertAsync(UpsertMenuEntryRequest request);
    Task<Result> DeleteAsync(Guid id);
}

public record UpsertMenuEntryRequest(DateOnly Date, Guid? RecipeId, bool HasDelivery, string DeliveryNote, bool IsConsumed, bool IsEatingOut);
