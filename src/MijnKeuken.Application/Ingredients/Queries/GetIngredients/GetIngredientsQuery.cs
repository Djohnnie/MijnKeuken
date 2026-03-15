using MediatR;
using MijnKeuken.Application.Ingredients.DTOs;

namespace MijnKeuken.Application.Ingredients.Queries.GetIngredients;

public record GetIngredientsQuery : IRequest<List<IngredientDto>>;
