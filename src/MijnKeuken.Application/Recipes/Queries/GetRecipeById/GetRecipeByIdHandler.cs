using MediatR;
using MijnKeuken.Application.Interfaces;
using MijnKeuken.Application.Recipes.DTOs;

namespace MijnKeuken.Application.Recipes.Queries.GetRecipeById;

public class GetRecipeByIdHandler(IRecipeRepository repository)
    : IRequestHandler<GetRecipeByIdQuery, RecipeDto?>
{
    public async Task<RecipeDto?> Handle(GetRecipeByIdQuery request, CancellationToken cancellationToken)
    {
        var recipe = await repository.GetByIdAsync(request.Id, cancellationToken);
        if (recipe is null) return null;

        return new RecipeDto(
            recipe.Id,
            recipe.Title,
            recipe.Description,
            recipe.Plan,
            recipe.Servings,
            recipe.SourceUrl,
            recipe.RecipeTags.Select(rt => new RecipeTagDto(rt.TagId, rt.Tag.Name, rt.Tag.Color)).ToList(),
            recipe.RecipeIngredients.OrderBy(ri => ri.SortOrder).Select(ri => new RecipeIngredientDto(
                ri.IngredientId,
                ri.Ingredient?.Title ?? ri.FreeText,
                ri.FreeText,
                ri.IsManaged,
                ri.Amount,
                ri.Unit,
                ri.CustomUnitDescription,
                ri.SortOrder)).ToList()
        );
    }
}
