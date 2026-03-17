using MediatR;
using MijnKeuken.Application.Common;

namespace MijnKeuken.Application.Ingredients.Commands.ArchiveIngredient;

public record ArchiveIngredientCommand(Guid Id) : IRequest<Result>;
