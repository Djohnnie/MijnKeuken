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
            r.RecipeTags.Select(rt => new RecipeTagDto(rt.TagId, rt.Tag.Name, rt.Tag.Color)).ToList(),
            r.RecipeIngredients.Select(ri => new RecipeIngredientDto(
                ri.IngredientId,
                ri.Ingredient.Title,
                ri.Amount,
                ri.Unit,
                ri.CustomUnitDescription)).ToList()
        )).ToList();
    }
}
