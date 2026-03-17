using MediatR;
using MijnKeuken.Application.Common;
using MijnKeuken.Application.Interfaces;

namespace MijnKeuken.Application.Ingredients.Commands.UnarchiveIngredient;

public class UnarchiveIngredientHandler(IIngredientRepository repository)
    : IRequestHandler<UnarchiveIngredientCommand, Result>
{
    public async Task<Result> Handle(UnarchiveIngredientCommand request, CancellationToken cancellationToken)
    {
        var ingredient = await repository.GetByIdAsync(request.Id, cancellationToken);
        if (ingredient is null)
            return Result.Failure("Ingrediënt niet gevonden.");

        ingredient.IsArchived = false;
        await repository.UpdateAsync(ingredient, cancellationToken);
        return Result.Success();
    }
}
