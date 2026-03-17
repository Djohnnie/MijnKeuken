using MediatR;
using MijnKeuken.Application.Common;

namespace MijnKeuken.Application.Recipes.Commands.UnarchiveRecipe;

public record UnarchiveRecipeCommand(Guid Id) : IRequest<Result>;
