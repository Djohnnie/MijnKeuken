using MediatR;
using MijnKeuken.Application.Common;
using MijnKeuken.Application.Interfaces;
using MijnKeuken.Domain.Entities;

namespace MijnKeuken.Application.Ingredients.Commands.UpdateIngredient;

public class UpdateIngredientHandler(IIngredientRepository repository)
    : IRequestHandler<UpdateIngredientCommand, Result>
{
    public async Task<Result> Handle(UpdateIngredientCommand request, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.Title))
            return Result.Failure("Titel is verplicht.");

        var ingredient = await repository.GetByIdAsync(request.Id, cancellationToken);
        if (ingredient is null)
            return Result.Failure("Ingrediënt niet gevonden.");

        ingredient.Title = request.Title.Trim();
        ingredient.Description = request.Description?.Trim() ?? string.Empty;
        ingredient.AmountAvailable = request.AmountAvailable;
        ingredient.AmountTotal = request.AmountTotal;
        ingredient.Unit = request.Unit;
        ingredient.CustomUnitDescription = request.CustomUnitDescription?.Trim() ?? string.Empty;
        ingredient.Barcode = request.Barcode?.Trim() ?? string.Empty;
        ingredient.StoreUrl = request.StoreUrl?.Trim() ?? string.Empty;
        ingredient.IsOutOfStock = request.IsOutOfStock;
        ingredient.StorageLocationId = request.StorageLocationId;

        // Replace tag associations
        ingredient.IngredientTags.Clear();
        ingredient.IngredientTags.AddRange(request.TagIds.Select(tagId => new IngredientTag
        {
            IngredientId = ingredient.Id,
            TagId = tagId
        }));

        await repository.UpdateAsync(ingredient, cancellationToken);

        return Result.Success();
    }
}
