using MediatR;
using MijnKeuken.Application.Archive.DTOs;
using MijnKeuken.Application.Interfaces;

namespace MijnKeuken.Application.Archive.Queries.GetArchivedItems;

public class GetArchivedItemsHandler(
    IRecipeRepository recipeRepository,
    IIngredientRepository ingredientRepository)
    : IRequestHandler<GetArchivedItemsQuery, List<ArchivedItemDto>>
{
    public async Task<List<ArchivedItemDto>> Handle(GetArchivedItemsQuery request, CancellationToken cancellationToken)
    {
        var recipes = await recipeRepository.GetArchivedAsync(cancellationToken);
        var ingredients = await ingredientRepository.GetArchivedAsync(cancellationToken);

        var items = new List<ArchivedItemDto>();

        items.AddRange(recipes.Select(r =>
            new ArchivedItemDto(r.Id, r.Title, ArchivedItemType.Recipe)));

        items.AddRange(ingredients.Select(i =>
            new ArchivedItemDto(i.Id, i.Title, ArchivedItemType.Ingredient)));

        return items.OrderBy(i => i.Title).ToList();
    }
}
