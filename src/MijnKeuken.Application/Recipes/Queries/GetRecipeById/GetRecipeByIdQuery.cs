using MediatR;
using MijnKeuken.Application.Recipes.DTOs;

namespace MijnKeuken.Application.Recipes.Queries.GetRecipeById;

public record GetRecipeByIdQuery(Guid Id) : IRequest<RecipeDto?>;
