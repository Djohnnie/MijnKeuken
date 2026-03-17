using MediatR;
using MijnKeuken.Application.Common;

namespace MijnKeuken.Application.Recipes.Commands.ArchiveRecipe;

public record ArchiveRecipeCommand(Guid Id) : IRequest<Result>;
