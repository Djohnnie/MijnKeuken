namespace MijnKeuken.Application.Menu.DTOs;

public record MenuEntryDto(
    Guid Id,
    DateOnly Date,
    Guid? RecipeId,
    string? RecipeTitle,
    bool HasDelivery,
    string DeliveryNote,
    bool IsConsumed,
    bool IsEatingOut);
