using MediatR;
using MijnKeuken.Application.Common;
using MijnKeuken.Domain.Entities;

namespace MijnKeuken.Application.Recipes.Commands.CreateRecipe;

public record RecipeIngredientInput(Guid? IngredientId, string FreeText, decimal Amount, UnitType Unit, string CustomUnitDescription, int SortOrder);

public record CreateRecipeCommand(
    string Title,
    string Description,
    string Plan,
    int Servings,
    string SourceUrl,
    List<Guid> TagIds,
    List<RecipeIngredientInput> Ingredients) : IRequest<Result<Guid>>;
