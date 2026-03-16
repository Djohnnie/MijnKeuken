using MediatR;
using MijnKeuken.Application.Common;
using MijnKeuken.Application.Interfaces;
using MijnKeuken.Application.Recipes.DTOs;

namespace MijnKeuken.Application.Recipes.Queries.ScrapeRecipeFromImage;

public class ScrapeRecipeFromImageHandler(
    IRecipeImageScraperService imageScraperService,
    IIngredientRepository ingredientRepository,
    IIngredientMatchingService ingredientMatchingService)
    : IRequestHandler<ScrapeRecipeFromImageQuery, Result<ScrapedRecipeDto>>
{
    private static readonly HashSet<string> AllowedContentTypes =
    [
        "image/jpeg",
        "image/png",
        "image/gif",
        "image/webp"
    ];

    public async Task<Result<ScrapedRecipeDto>> Handle(
        ScrapeRecipeFromImageQuery request,
        CancellationToken cancellationToken)
    {
        if (request.ImageData.Length == 0)
            return Result<ScrapedRecipeDto>.Failure("Geen afbeelding ontvangen.");

        if (!AllowedContentTypes.Contains(request.ContentType))
            return Result<ScrapedRecipeDto>.Failure("Ongeldig bestandstype. Gebruik JPEG, PNG, GIF of WebP.");

        var scrapeResult = await imageScraperService.ScrapeAsync(
            request.ImageData, request.ContentType, cancellationToken);

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
