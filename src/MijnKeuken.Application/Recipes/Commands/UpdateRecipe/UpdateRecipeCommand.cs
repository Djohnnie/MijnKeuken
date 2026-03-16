using MediatR;
using MijnKeuken.Application.Common;
using MijnKeuken.Application.Recipes.Commands.CreateRecipe;
using MijnKeuken.Domain.Entities;

namespace MijnKeuken.Application.Recipes.Commands.UpdateRecipe;

public record UpdateRecipeCommand(
    Guid Id,
    string Title,
    string Description,
    string Plan,
    int Servings,
    string SourceUrl,
    List<Guid> TagIds,
    List<RecipeIngredientInput> Ingredients) : IRequest<Result>;
