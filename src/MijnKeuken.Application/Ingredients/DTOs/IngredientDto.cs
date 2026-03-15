using MijnKeuken.Domain.Entities;

namespace MijnKeuken.Application.Ingredients.DTOs;

public record IngredientDto(
    Guid Id,
    string Title,
    string Description,
    decimal AmountAvailable,
    decimal AmountTotal,
    UnitType Unit,
    string CustomUnitDescription,
    string Barcode,
    string StoreUrl,
    bool IsOutOfStock,
    Guid? StorageLocationId,
    string? StorageLocationName,
    List<IngredientTagDto> Tags);

public record IngredientTagDto(Guid TagId, string Name, string Color);
