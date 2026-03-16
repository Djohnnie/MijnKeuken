using MediatR;
using MijnKeuken.Application.Common;
using MijnKeuken.Application.Interfaces;
using MijnKeuken.Application.Recipes.DTOs;

namespace MijnKeuken.Application.Recipes.Queries.ScrapeRecipe;

public class ScrapeRecipeFromUrlHandler(IRecipeScraperService scraperService)
    : IRequestHandler<ScrapeRecipeFromUrlQuery, Result<ScrapedRecipeDto>>
{
    public async Task<Result<ScrapedRecipeDto>> Handle(
        ScrapeRecipeFromUrlQuery request,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.Url))
            return Result<ScrapedRecipeDto>.Failure("URL is verplicht.");

        if (!Uri.TryCreate(request.Url.Trim(), UriKind.Absolute, out var uri)
            || (uri.Scheme != Uri.UriSchemeHttp && uri.Scheme != Uri.UriSchemeHttps))
            return Result<ScrapedRecipeDto>.Failure("Ongeldige URL.");

        return await scraperService.ScrapeAsync(request.Url.Trim(), cancellationToken);
    }
}
