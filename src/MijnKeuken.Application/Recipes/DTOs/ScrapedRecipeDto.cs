using System.ComponentModel;

namespace MijnKeuken.Application.Recipes.DTOs;

public record ScrapedRecipeDto(
    [property: Description("The recipe title or name")]
    string Title,
    [property: Description("A short description of the recipe")]
    string Description,
    [property: Description("The full cooking instructions formatted as Markdown with numbered steps")]
    string Plan,
    [property: Description("The number of servings/persons this recipe is intended for, or null if not specified")]
    int? Servings,
    [property: Description("List of ingredients with their amounts and units")]
    List<ScrapedRecipeIngredientDto> Ingredients);

public record ScrapedRecipeIngredientDto(
    [property: Description("The ingredient name")]
    string Name,
    [property: Description("The amount/quantity needed (e.g. 200 for 200g, 2 for 2 pieces)")]
    decimal Amount,
    [property: Description("The unit: Grams for weight-based (g, kg, ml, l), Units for countable items (stuks, el, tl, snuf, bos)")]
    string Unit);
