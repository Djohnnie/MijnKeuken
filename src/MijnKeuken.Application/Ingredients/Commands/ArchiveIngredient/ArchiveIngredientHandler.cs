using MediatR;
using MijnKeuken.Application.Common;
using MijnKeuken.Application.Interfaces;

namespace MijnKeuken.Application.Ingredients.Commands.ArchiveIngredient;

public class ArchiveIngredientHandler(IIngredientRepository repository)
    : IRequestHandler<ArchiveIngredientCommand, Result>
{
    public async Task<Result> Handle(ArchiveIngredientCommand request, CancellationToken cancellationToken)
    {
        var ingredient = await repository.GetByIdAsync(request.Id, cancellationToken);
        if (ingredient is null)
            return Result.Failure("Ingrediënt niet gevonden.");

        ingredient.IsArchived = true;
        await repository.UpdateAsync(ingredient, cancellationToken);
        return Result.Success();
    }
}
