namespace MijnKeuken.Domain.Entities;

public class IngredientTag
{
    public Guid IngredientId { get; set; }
    public Ingredient Ingredient { get; set; } = null!;

    public Guid TagId { get; set; }
    public Tag Tag { get; set; } = null!;
}
