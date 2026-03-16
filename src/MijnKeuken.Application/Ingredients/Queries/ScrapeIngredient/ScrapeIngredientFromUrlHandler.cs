using MediatR;
using MijnKeuken.Application.Common;
using MijnKeuken.Application.Ingredients.DTOs;
using MijnKeuken.Application.Interfaces;

namespace MijnKeuken.Application.Ingredients.Queries.ScrapeIngredient;

public class ScrapeIngredientFromUrlHandler(IIngredientScraperService scraperService)
    : IRequestHandler<ScrapeIngredientFromUrlQuery, Result<ScrapedIngredientDto>>
{
    public async Task<Result<ScrapedIngredientDto>> Handle(
        ScrapeIngredientFromUrlQuery request,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.Url))
            return Result<ScrapedIngredientDto>.Failure("URL is verplicht.");

        if (!Uri.TryCreate(request.Url.Trim(), UriKind.Absolute, out var uri)
            || (uri.Scheme != Uri.UriSchemeHttp && uri.Scheme != Uri.UriSchemeHttps))
            return Result<ScrapedIngredientDto>.Failure("Ongeldige URL.");

        return await scraperService.ScrapeAsync(request.Url.Trim(), cancellationToken);
    }
}
