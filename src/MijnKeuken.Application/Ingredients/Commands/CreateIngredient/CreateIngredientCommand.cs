using MediatR;
using MijnKeuken.Application.Common;
using MijnKeuken.Domain.Entities;

namespace MijnKeuken.Application.Ingredients.Commands.CreateIngredient;

public record CreateIngredientCommand(
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
    List<Guid> TagIds) : IRequest<Result<Guid>>;
