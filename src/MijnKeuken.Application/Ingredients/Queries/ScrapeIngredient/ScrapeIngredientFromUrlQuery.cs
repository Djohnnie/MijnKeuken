using MediatR;
using MijnKeuken.Application.Common;
using MijnKeuken.Application.Ingredients.DTOs;

namespace MijnKeuken.Application.Ingredients.Queries.ScrapeIngredient;

public record ScrapeIngredientFromUrlQuery(string Url) : IRequest<Result<ScrapedIngredientDto>>;
