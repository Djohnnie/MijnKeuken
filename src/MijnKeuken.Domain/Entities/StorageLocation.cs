namespace MijnKeuken.Domain.Entities;

public class StorageLocation : BaseEntity
{
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
}
