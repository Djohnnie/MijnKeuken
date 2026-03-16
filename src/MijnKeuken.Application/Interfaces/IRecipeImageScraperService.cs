using MijnKeuken.Application.Common;
using MijnKeuken.Application.Recipes.DTOs;

namespace MijnKeuken.Application.Interfaces;

public interface IRecipeImageScraperService
{
    Task<Result<ScrapedRecipeDto>> ScrapeAsync(
        byte[] imageData,
        string contentType,
        CancellationToken cancellationToken = default);
}
