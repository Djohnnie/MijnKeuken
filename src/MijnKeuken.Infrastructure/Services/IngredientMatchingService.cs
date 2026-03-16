using System.ComponentModel;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.Logging;
using MijnKeuken.Application.Interfaces;
using MijnKeuken.Domain.Entities;

namespace MijnKeuken.Infrastructure.Services;

public class IngredientMatchingService(
    IChatClient chatClient,
    ILogger<IngredientMatchingService> logger) : IIngredientMatchingService
{
    public async Task<Dictionary<string, Guid>> MatchAsync(
        List<string> scrapedNames,
        List<Ingredient> storedIngredients,
        CancellationToken ct = default)
    {
        if (scrapedNames.Count == 0 || storedIngredients.Count == 0)
            return [];

        var titleToId = storedIngredients.ToDictionary(i => i.Title, i => i.Id);
        var storedTitles = string.Join("\n", storedIngredients.Select(i => $"- {i.Title}"));
        var scrapedList = string.Join("\n", scrapedNames.Select(n => $"- {n}"));

        try
        {
            var response = await chatClient.GetResponseAsync<IngredientMatchResponse>(
                [
                    new ChatMessage(ChatRole.System, $"""
                        You are an ingredient matching assistant. You will be given two lists:
                        1. A list of stored ingredient names (from a kitchen inventory)
                        2. A list of scraped ingredient names (from a recipe website)

                        For each scraped ingredient, find the best matching stored ingredient.
                        Only match when the ingredients are clearly the same product, accounting for:
                        - Different spelling or casing (e.g. "kipfilet" = "Kipfilet")
                        - Singular vs plural (e.g. "ui" = "Uien", "tomaat" = "Tomaten")
                        - Minor wording differences (e.g. "rode paprika" = "Paprika rood")
                        - Abbreviations or common variations (e.g. "aardappelen" = "Aardappels")

                        Do NOT match when:
                        - The ingredients are fundamentally different products
                        - The match would be too ambiguous

                        STORED INGREDIENTS:
                        {storedTitles}
                        """),
                    new ChatMessage(ChatRole.User, $"""
                        Match these scraped ingredients to the stored list above:
                        {scrapedList}
                        """)
                ],
                cancellationToken: ct);

            if (response.Result?.Matches is not { Count: > 0 })
                return [];

            var result = new Dictionary<string, Guid>();
            foreach (var match in response.Result.Matches)
            {
                if (!string.IsNullOrWhiteSpace(match.StoredTitle)
                    && titleToId.TryGetValue(match.StoredTitle, out var id))
                {
                    result[match.ScrapedName] = id;
                }
            }

            return result;
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "LLM ingredient matching failed, falling back to no matches");
            return [];
        }
    }
}

internal record IngredientMatchResponse(
    [property: Description("List of matched ingredients")]
    List<IngredientMatchItem> Matches);

internal record IngredientMatchItem(
    [property: Description("The original scraped ingredient name, exactly as provided")]
    string ScrapedName,
    [property: Description("The exact stored ingredient title that best matches, or empty string if no good match")]
    string StoredTitle);
