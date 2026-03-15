using MediatR;
using MijnKeuken.Application.Common;

namespace MijnKeuken.Application.Ingredients.Commands.DeleteIngredient;

public record DeleteIngredientCommand(Guid Id) : IRequest<Result>;
