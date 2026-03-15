using MediatR;
using MijnKeuken.Application.Common;
using MijnKeuken.Application.Interfaces;
using MijnKeuken.Domain.Entities;

namespace MijnKeuken.Application.Ingredients.Commands.CreateIngredient;

public class CreateIngredientHandler(IIngredientRepository repository)
    : IRequestHandler<CreateIngredientCommand, Result<Guid>>
{
    public async Task<Result<Guid>> Handle(CreateIngredientCommand request, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.Title))
            return Result<Guid>.Failure("Titel is verplicht.");

        if (await repository.ExistsByTitleAsync(request.Title.Trim(), cancellationToken))
            return Result<Guid>.Failure("Er bestaat al een ingrediënt met deze titel.");

        var ingredient = new Ingredient
        {
            Id = Guid.NewGuid(),
            Title = request.Title.Trim(),
            Description = request.Description?.Trim() ?? string.Empty,
            AmountAvailable = request.AmountAvailable,
            AmountTotal = request.AmountTotal,
            Unit = request.Unit,
            CustomUnitDescription = request.CustomUnitDescription?.Trim() ?? string.Empty,
            Barcode = request.Barcode?.Trim() ?? string.Empty,
            StoreUrl = request.StoreUrl?.Trim() ?? string.Empty,
            IsOutOfStock = request.IsOutOfStock,
            StorageLocationId = request.StorageLocationId,
            IngredientTags = request.TagIds.Select(tagId => new IngredientTag
            {
                IngredientId = default,
                TagId = tagId
            }).ToList()
        };

        // Set the IngredientId after we have the Guid
        foreach (var it in ingredient.IngredientTags)
            it.IngredientId = ingredient.Id;

        await repository.AddAsync(ingredient, cancellationToken);

        return Result<Guid>.Success(ingredient.Id);
    }
}
