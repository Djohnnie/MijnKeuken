namespace MijnKeuken.Domain.Entities;

public class Tag : BaseEntity
{
    public string Name { get; set; } = string.Empty;
    public TagType Type { get; set; }
    public string Color { get; set; } = "#1976d2";
}
