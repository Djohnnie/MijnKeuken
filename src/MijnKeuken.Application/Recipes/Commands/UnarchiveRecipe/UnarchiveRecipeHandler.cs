using MediatR;
using MijnKeuken.Application.Common;
using MijnKeuken.Application.Interfaces;

namespace MijnKeuken.Application.Recipes.Commands.UnarchiveRecipe;

public class UnarchiveRecipeHandler(IRecipeRepository repository)
    : IRequestHandler<UnarchiveRecipeCommand, Result>
{
    public async Task<Result> Handle(UnarchiveRecipeCommand request, CancellationToken cancellationToken)
    {
        var recipe = await repository.GetByIdAsync(request.Id, cancellationToken);
        if (recipe is null)
            return Result.Failure("Recept niet gevonden.");

        recipe.IsArchived = false;
        await repository.UpdateAsync(recipe, cancellationToken);
        return Result.Success();
    }
}
