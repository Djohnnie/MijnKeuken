using MediatR;
using MijnKeuken.Application.Common;
using MijnKeuken.Application.Interfaces;
using MijnKeuken.Domain.Entities;

namespace MijnKeuken.Application.Recipes.Commands.CreateRecipe;

public class CreateRecipeHandler(IRecipeRepository repository)
    : IRequestHandler<CreateRecipeCommand, Result<Guid>>
{
    public async Task<Result<Guid>> Handle(CreateRecipeCommand request, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.Title))
            return Result<Guid>.Failure("Titel is verplicht.");

        if (request.Servings <= 0)
            return Result<Guid>.Failure("Aantal personen moet groter zijn dan 0.");

        if (request.Ingredients.Any(i => i.Amount <= 0))
            return Result<Guid>.Failure("Hoeveelheid moet groter zijn dan 0.");

        if (await repository.ExistsByTitleAsync(request.Title.Trim(), cancellationToken))
            return Result<Guid>.Failure("Er bestaat al een recept met deze titel.");

        var recipe = new Recipe
        {
            Id = Guid.NewGuid(),
            Title = request.Title.Trim(),
            Description = request.Description?.Trim() ?? string.Empty,
            Plan = request.Plan ?? string.Empty,
            Servings = request.Servings,
            SourceUrl = request.SourceUrl?.Trim() ?? string.Empty,
            RecipeTags = request.TagIds.Select(tagId => new RecipeTag
            {
                TagId = tagId
            }).ToList(),
            RecipeIngredients = request.Ingredients.Select(i => new RecipeIngredient
            {
                Id = Guid.NewGuid(),
                IngredientId = i.IngredientId,
                FreeText = i.FreeText?.Trim() ?? string.Empty,
                Amount = i.Amount,
                Unit = i.Unit,
                CustomUnitDescription = i.CustomUnitDescription?.Trim() ?? string.Empty,
                SortOrder = i.SortOrder
            }).ToList()
        };

        foreach (var rt in recipe.RecipeTags)
            rt.RecipeId = recipe.Id;
        foreach (var ri in recipe.RecipeIngredients)
            ri.RecipeId = recipe.Id;

        await repository.AddAsync(recipe, cancellationToken);
        return Result<Guid>.Success(recipe.Id);
    }
}
