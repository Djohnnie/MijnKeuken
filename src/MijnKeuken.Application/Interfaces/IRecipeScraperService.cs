using MijnKeuken.Application.Common;
using MijnKeuken.Application.Recipes.DTOs;

namespace MijnKeuken.Application.Interfaces;

public interface IRecipeScraperService
{
    Task<Result<ScrapedRecipeDto>> ScrapeAsync(string url, CancellationToken cancellationToken = default);
}
