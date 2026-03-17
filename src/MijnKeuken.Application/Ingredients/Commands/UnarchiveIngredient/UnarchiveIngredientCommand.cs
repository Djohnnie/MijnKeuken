using MediatR;
using MijnKeuken.Application.Common;

namespace MijnKeuken.Application.Ingredients.Commands.UnarchiveIngredient;

public record UnarchiveIngredientCommand(Guid Id) : IRequest<Result>;
