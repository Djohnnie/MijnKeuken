namespace MijnKeuken.Domain.Entities;

public class Ingredient : BaseEntity
{
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public decimal AmountAvailable { get; set; }
    public decimal AmountTotal { get; set; }
    public UnitType Unit { get; set; }
    public string CustomUnitDescription { get; set; } = string.Empty;
    public string Barcode { get; set; } = string.Empty;
    public string StoreUrl { get; set; } = string.Empty;
    public bool IsOutOfStock { get; set; }

    public Guid? StorageLocationId { get; set; }
    public StorageLocation? StorageLocation { get; set; }

    public List<IngredientTag> IngredientTags { get; set; } = [];
}
