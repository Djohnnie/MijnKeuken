using MediatR;
using MijnKeuken.Application.Common;
using MijnKeuken.Application.Interfaces;

namespace MijnKeuken.Application.Ingredients.Commands.DeleteIngredient;

public class DeleteIngredientHandler(IIngredientRepository repository)
    : IRequestHandler<DeleteIngredientCommand, Result>
{
    public async Task<Result> Handle(DeleteIngredientCommand request, CancellationToken cancellationToken)
    {
        var ingredient = await repository.GetByIdAsync(request.Id, cancellationToken);
        if (ingredient is null)
            return Result.Failure("Ingrediënt niet gevonden.");

        await repository.DeleteAsync(ingredient, cancellationToken);

        return Result.Success();
    }
}
