using MediatR;
using MijnKeuken.Application.Common;
using MijnKeuken.Domain.Entities;

namespace MijnKeuken.Application.Recipes.Commands.CreateRecipe;

public record RecipeIngredientInput(Guid IngredientId, decimal Amount, UnitType Unit, string CustomUnitDescription);

public record CreateRecipeCommand(
    string Title,
    string Description,
    string Plan,
    List<Guid> TagIds,
    List<RecipeIngredientInput> Ingredients) : IRequest<Result<Guid>>;
