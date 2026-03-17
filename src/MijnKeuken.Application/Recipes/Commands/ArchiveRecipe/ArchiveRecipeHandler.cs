using MediatR;
using MijnKeuken.Application.Common;
using MijnKeuken.Application.Interfaces;

namespace MijnKeuken.Application.Recipes.Commands.ArchiveRecipe;

public class ArchiveRecipeHandler(IRecipeRepository repository)
    : IRequestHandler<ArchiveRecipeCommand, Result>
{
    public async Task<Result> Handle(ArchiveRecipeCommand request, CancellationToken cancellationToken)
    {
        var recipe = await repository.GetByIdAsync(request.Id, cancellationToken);
        if (recipe is null)
            return Result.Failure("Recept niet gevonden.");

        recipe.IsArchived = true;
        await repository.UpdateAsync(recipe, cancellationToken);
        return Result.Success();
    }
}
