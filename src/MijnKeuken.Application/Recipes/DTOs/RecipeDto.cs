using MijnKeuken.Domain.Entities;

namespace MijnKeuken.Application.Recipes.DTOs;

public record RecipeDto(
    Guid Id,
    string Title,
    string Description,
    string Plan,
    List<RecipeTagDto> Tags,
    List<RecipeIngredientDto> Ingredients);

public record RecipeTagDto(Guid TagId, string Name, string Color);

public record RecipeIngredientDto(
    Guid? IngredientId,
    string IngredientTitle,
    string FreeText,
    bool IsManaged,
    decimal Amount,
    UnitType Unit,
    string CustomUnitDescription);
