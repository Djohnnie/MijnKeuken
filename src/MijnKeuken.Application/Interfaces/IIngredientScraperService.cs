using MijnKeuken.Application.Common;
using MijnKeuken.Application.Ingredients.DTOs;

namespace MijnKeuken.Application.Interfaces;

public interface IIngredientScraperService
{
    Task<Result<ScrapedIngredientDto>> ScrapeAsync(string url, CancellationToken cancellationToken = default);
}
