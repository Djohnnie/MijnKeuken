using MediatR;
using MijnKeuken.Application.Common;

namespace MijnKeuken.Application.Recipes.Commands.DeleteRecipe;

public record DeleteRecipeCommand(Guid Id) : IRequest<Result>;
