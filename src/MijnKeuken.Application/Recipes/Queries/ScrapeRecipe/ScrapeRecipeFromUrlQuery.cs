using MediatR;
using MijnKeuken.Application.Common;
using MijnKeuken.Application.Recipes.DTOs;

namespace MijnKeuken.Application.Recipes.Queries.ScrapeRecipe;

public record ScrapeRecipeFromUrlQuery(string Url) : IRequest<Result<ScrapedRecipeDto>>;
