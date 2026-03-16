namespace MijnKeuken.Domain.Entities;

public class RecipeIngredient
{
    public Guid Id { get; set; }
    public Guid RecipeId { get; set; }
    public Recipe Recipe { get; set; } = null!;

    public Guid? IngredientId { get; set; }
    public Ingredient? Ingredient { get; set; }

    public string FreeText { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public UnitType Unit { get; set; }
    public string CustomUnitDescription { get; set; } = string.Empty;

    public bool IsManaged => IngredientId.HasValue;
}
