using MediatR;
using MijnKeuken.Application.Interfaces;
using MijnKeuken.Application.Recipes.DTOs;

namespace MijnKeuken.Application.Recipes.Queries.GetRecipes;

public class GetRecipesHandler(IRecipeRepository repository)
    : IRequestHandler<GetRecipesQuery, List<RecipeDto>>
{
    public async Task<List<RecipeDto>> Handle(GetRecipesQuery request, CancellationToken cancellationToken)
    {
        var recipes = await repository.GetAllAsync(cancellationToken);
        return recipes.Select(r => new RecipeDto(
            r.Id,
            r.Title,
            r.Description,
            r.Plan,
            r.Servings,
            r.SourceUrl,
            r.RecipeTags.Select(rt => new RecipeTagDto(rt.TagId, rt.Tag.Name, rt.Tag.Color)).ToList(),
            r.RecipeIngredients.OrderBy(ri => ri.SortOrder).Select(ri => new RecipeIngredientDto(
                ri.IngredientId,
                ri.Ingredient?.Title ?? ri.FreeText,
                ri.FreeText,
                ri.IsManaged,
                ri.Amount,
                ri.Unit,
                ri.CustomUnitDescription,
                ri.SortOrder)).ToList()
        )).ToList();
    }
}
