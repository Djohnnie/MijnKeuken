using System.Text.RegularExpressions;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.Logging;
using MijnKeuken.Application.Common;
using MijnKeuken.Application.Interfaces;
using MijnKeuken.Application.Recipes.DTOs;

namespace MijnKeuken.Infrastructure.Services;

public partial class RecipeScraperService(
    IChatClient chatClient,
    IHttpClientFactory httpClientFactory,
    ILogger<RecipeScraperService> logger) : IRecipeScraperService
{
    private const int MaxHtmlLength = 50_000;

    public async Task<Result<ScrapedRecipeDto>> ScrapeAsync(
        string url,
        CancellationToken cancellationToken = default)
    {
        string pageContent;
        try
        {
            pageContent = await FetchPageContentAsync(url, cancellationToken);
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "Failed to fetch URL {Url}", url);
            return Result<ScrapedRecipeDto>.Failure("Kon de webpagina niet ophalen. Controleer de URL.");
        }

        try
        {
            var scraped = await ExtractRecipeDataAsync(pageContent, cancellationToken);
            return scraped is not null
                ? Result<ScrapedRecipeDto>.Success(scraped)
                : Result<ScrapedRecipeDto>.Failure("Kon geen receptgegevens extraheren uit de pagina.");
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "Failed to extract recipe data from {Url}", url);
            return Result<ScrapedRecipeDto>.Failure("Er ging iets mis bij het analyseren van de pagina.");
        }
    }

    private async Task<string> FetchPageContentAsync(string url, CancellationToken cancellationToken)
    {
        using var client = httpClientFactory.CreateClient("RecipeScraper");
        client.DefaultRequestHeaders.UserAgent.ParseAdd(
            "Mozilla/5.0 (compatible; MijnKeuken/1.0)");
        client.Timeout = TimeSpan.FromSeconds(15);

        var html = await client.GetStringAsync(url, cancellationToken);

        html = ScriptPattern().Replace(html, " ");
        html = StylePattern().Replace(html, " ");
        html = TagPattern().Replace(html, " ");
        html = WhitespacePattern().Replace(html, " ").Trim();

        if (html.Length > MaxHtmlLength)
            html = html[..MaxHtmlLength];

        return html;
    }

    private async Task<ScrapedRecipeDto?> ExtractRecipeDataAsync(
        string pageContent,
        CancellationToken cancellationToken)
    {
        var response = await chatClient.GetResponseAsync<ScrapedRecipeDto>(
            [
                new ChatMessage(ChatRole.System, """
                    You are a recipe data extractor. Given the text content of a recipe webpage,
                    extract the following information:
                    - Title: the recipe name
                    - Description: a short description of the dish
                    - Plan: the full cooking instructions formatted as Markdown.
                      Use numbered steps (1. 2. 3.) with clear headings (## Voorbereiding, ## Bereiding)
                      where appropriate.
                    - Ingredients: a list of ingredients, each with:
                      - Name: ingredient name
                      - Amount: numeric quantity (e.g. 500 for 500g, 2 for 2 pieces, 0 if unspecified)
                      - Unit: the unit as text (e.g. "gram", "stuks", "el", "tl", "ml", "snuf", "bos", "teen")
                    Use Dutch as the output language!
                    If you cannot determine a value, use a sensible default.
                    """),
                new ChatMessage(ChatRole.User, pageContent)
            ],
            cancellationToken: cancellationToken);

        return response.Result;
    }

    [GeneratedRegex(@"<script[^>]*>[\s\S]*?</script>", RegexOptions.IgnoreCase)]
    private static partial Regex ScriptPattern();

    [GeneratedRegex(@"<style[^>]*>[\s\S]*?</style>", RegexOptions.IgnoreCase)]
    private static partial Regex StylePattern();

    [GeneratedRegex(@"<[^>]+>")]
    private static partial Regex TagPattern();

    [GeneratedRegex(@"\s{2,}")]
    private static partial Regex WhitespacePattern();
}
