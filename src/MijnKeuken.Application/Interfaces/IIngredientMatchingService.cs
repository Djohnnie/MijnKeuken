using MijnKeuken.Domain.Entities;

namespace MijnKeuken.Application.Interfaces;

public interface IIngredientMatchingService
{
    Task<Dictionary<string, Guid>> MatchAsync(
        List<string> scrapedNames,
        List<Ingredient> storedIngredients,
        CancellationToken ct = default);
}
