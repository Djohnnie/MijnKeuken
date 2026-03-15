using MediatR;
using MijnKeuken.Application.Recipes.DTOs;

namespace MijnKeuken.Application.Recipes.Queries.GetRecipes;

public record GetRecipesQuery : IRequest<List<RecipeDto>>;
