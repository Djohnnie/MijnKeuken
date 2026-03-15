using MediatR;
using MijnKeuken.Application.Common;
using MijnKeuken.Application.Interfaces;

namespace MijnKeuken.Application.Recipes.Commands.DeleteRecipe;

public class DeleteRecipeHandler(IRecipeRepository repository)
    : IRequestHandler<DeleteRecipeCommand, Result>
{
    public async Task<Result> Handle(DeleteRecipeCommand request, CancellationToken cancellationToken)
    {
        var recipe = await repository.GetByIdAsync(request.Id, cancellationToken);
        if (recipe is null)
            return Result.Failure("Recept niet gevonden.");

        await repository.DeleteAsync(recipe, cancellationToken);
        return Result.Success();
    }
}
