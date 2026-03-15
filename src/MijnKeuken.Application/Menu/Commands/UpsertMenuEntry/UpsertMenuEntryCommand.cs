using MediatR;
using MijnKeuken.Application.Common;

namespace MijnKeuken.Application.Menu.Commands.UpsertMenuEntry;

public record UpsertMenuEntryCommand(
    DateOnly Date,
    Guid? RecipeId,
    bool HasDelivery,
    string DeliveryNote,
    bool IsConsumed,
    bool IsEatingOut) : IRequest<Result<Guid>>;
