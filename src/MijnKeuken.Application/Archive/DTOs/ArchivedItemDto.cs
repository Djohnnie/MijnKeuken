namespace MijnKeuken.Application.Archive.DTOs;

public record ArchivedItemDto(Guid Id, string Title, ArchivedItemType Type);

public enum ArchivedItemType
{
    Recipe,
    Ingredient
}
