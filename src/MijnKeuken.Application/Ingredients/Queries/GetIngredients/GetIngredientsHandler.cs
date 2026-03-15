using MediatR;
using MijnKeuken.Application.Interfaces;
using MijnKeuken.Application.Ingredients.DTOs;

namespace MijnKeuken.Application.Ingredients.Queries.GetIngredients;

public class GetIngredientsHandler(IIngredientRepository repository)
    : IRequestHandler<GetIngredientsQuery, List<IngredientDto>>
{
    public async Task<List<IngredientDto>> Handle(GetIngredientsQuery request, CancellationToken cancellationToken)
    {
        var ingredients = await repository.GetAllAsync(cancellationToken);
        return ingredients.Select(i => new IngredientDto(
            i.Id,
            i.Title,
            i.Description,
            i.AmountAvailable,
            i.AmountTotal,
            i.Unit,
            i.CustomUnitDescription,
            i.Barcode,
            i.StoreUrl,
            i.IsOutOfStock,
            i.StorageLocationId,
            i.StorageLocation?.Name,
            i.IngredientTags.Select(it => new IngredientTagDto(it.TagId, it.Tag.Name, it.Tag.Color)).ToList()
        )).ToList();
    }
}
