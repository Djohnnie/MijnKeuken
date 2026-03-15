namespace MijnKeuken.Domain.Entities;

public class MenuEntry : BaseEntity
{
    public DateOnly Date { get; set; }
    public Guid? RecipeId { get; set; }
    public Recipe? Recipe { get; set; }
    public bool HasDelivery { get; set; }
    public string DeliveryNote { get; set; } = string.Empty;
    public bool IsConsumed { get; set; }
    public bool IsEatingOut { get; set; }
}
