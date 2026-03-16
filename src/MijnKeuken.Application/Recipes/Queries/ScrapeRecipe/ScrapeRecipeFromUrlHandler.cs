using MediatR;
using MijnKeuken.Application.Common;
using MijnKeuken.Application.Interfaces;
using MijnKeuken.Application.Recipes.DTOs;

namespace MijnKeuken.Application.Recipes.Queries.ScrapeRecipe;

public class ScrapeRecipeFromUrlHandler(
    IRecipeScraperService scraperService,
    IIngredientRepository ingredientRepository,
    IIngredientMatchingService ingredientMatchingService)
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

        var scrapeResult = await scraperService.ScrapeAsync(request.Url.Trim(), cancellationToken);
        if (!scrapeResult.IsSuccess)
            return scrapeResult;

        var scraped = scrapeResult.Value!;
        var storedIngredients = await ingredientRepository.GetAllAsync(cancellationToken);

        if (storedIngredients.Count == 0 || scraped.Ingredients.Count == 0)
            return scrapeResult;

        var scrapedNames = scraped.Ingredients.Select(i => i.Name).ToList();
        var matches = await ingredientMatchingService.MatchAsync(scrapedNames, storedIngredients, cancellationToken);

        if (matches.Count == 0)
            return scrapeResult;

        var titleLookup = storedIngredients.ToDictionary(i => i.Id, i => i.Title);
        var enrichedIngredients = scraped.Ingredients.Select(i =>
        {
            if (matches.TryGetValue(i.Name, out var matchedId))
                return i with { MatchedIngredientId = matchedId, MatchedIngredientTitle = titleLookup[matchedId] };
            return i;
        }).ToList();

        return Result<ScrapedRecipeDto>.Success(scraped with { Ingredients = enrichedIngredients });
    }
}
