namespace MijnKeuken.Domain.Entities;

public abstract class BaseEntity
{
    public Guid Id { get; set; }
    public int SysId { get; set; }
}
