using MediatR;
using MijnKeuken.Application.Common;
using MijnKeuken.Application.Interfaces;
using MijnKeuken.Domain.Entities;

namespace MijnKeuken.Application.Menu.Commands.UpsertMenuEntry;

public class UpsertMenuEntryHandler(IMenuEntryRepository repository)
    : IRequestHandler<UpsertMenuEntryCommand, Result<Guid>>
{
    public async Task<Result<Guid>> Handle(UpsertMenuEntryCommand request, CancellationToken cancellationToken)
    {
        var existing = await repository.GetByDateAsync(request.Date, cancellationToken);

        if (request.RecipeId is null && !request.HasDelivery && !request.IsEatingOut)
        {
            if (existing is not null)
                await repository.DeleteAsync(existing, cancellationToken);

            return Result<Guid>.Success(Guid.Empty);
        }

        if (existing is not null)
        {
            existing.RecipeId = request.RecipeId;
            existing.HasDelivery = request.HasDelivery;
            existing.DeliveryNote = request.DeliveryNote;
            existing.IsConsumed = request.IsConsumed;
            existing.IsEatingOut = request.IsEatingOut;
            await repository.UpdateAsync(existing, cancellationToken);
            return Result<Guid>.Success(existing.Id);
        }

        var entry = new MenuEntry
        {
            Id = Guid.NewGuid(),
            Date = request.Date,
            RecipeId = request.RecipeId,
            HasDelivery = request.HasDelivery,
            DeliveryNote = request.DeliveryNote,
            IsConsumed = request.IsConsumed,
            IsEatingOut = request.IsEatingOut
        };

        await repository.AddAsync(entry, cancellationToken);
        return Result<Guid>.Success(entry.Id);
    }
}
