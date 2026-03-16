using MediatR;
using MijnKeuken.Application.Common;
using MijnKeuken.Application.Recipes.DTOs;

namespace MijnKeuken.Application.Recipes.Queries.ScrapeRecipeFromImage;

public record ScrapeRecipeFromImageQuery(byte[] ImageData, string ContentType)
    : IRequest<Result<ScrapedRecipeDto>>;
