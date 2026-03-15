using MediatR;
using MijnKeuken.Application.Common;
using MijnKeuken.Application.Interfaces;
using MijnKeuken.Application.Recipes.Commands.CreateRecipe;
using MijnKeuken.Domain.Entities;

namespace MijnKeuken.Application.Recipes.Commands.UpdateRecipe;

public class UpdateRecipeHandler(IRecipeRepository repository)
    : IRequestHandler<UpdateRecipeCommand, Result>
{
    public async Task<Result> Handle(UpdateRecipeCommand request, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.Title))
            return Result.Failure("Titel is verplicht.");

        var recipe = await repository.GetByIdAsync(request.Id, cancellationToken);
        if (recipe is null)
            return Result.Failure("Recept niet gevonden.");

        recipe.Title = request.Title.Trim();
        recipe.Description = request.Description?.Trim() ?? string.Empty;
        recipe.Plan = request.Plan ?? string.Empty;

        recipe.RecipeTags.Clear();
        recipe.RecipeTags.AddRange(request.TagIds.Select(tagId => new RecipeTag
        {
            RecipeId = recipe.Id,
            TagId = tagId
        }));

        recipe.RecipeIngredients.Clear();
        recipe.RecipeIngredients.AddRange(request.Ingredients.Select(i => new RecipeIngredient
        {
            RecipeId = recipe.Id,
            IngredientId = i.IngredientId,
            Amount = i.Amount,
            Unit = i.Unit,
            CustomUnitDescription = i.CustomUnitDescription?.Trim() ?? string.Empty
        }));

        await repository.UpdateAsync(recipe, cancellationToken);
        return Result.Success();
    }
}
