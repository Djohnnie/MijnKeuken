using System.Text.RegularExpressions;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.Logging;
using MijnKeuken.Application.Common;
using MijnKeuken.Application.Ingredients.DTOs;
using MijnKeuken.Application.Interfaces;

namespace MijnKeuken.Infrastructure.Services;

public partial class IngredientScraperService(
    IChatClient chatClient,
    IHttpClientFactory httpClientFactory,
    ILogger<IngredientScraperService> logger) : IIngredientScraperService
{
    private const int MaxHtmlLength = 50_000;

    public async Task<Result<ScrapedIngredientDto>> ScrapeAsync(
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
            return Result<ScrapedIngredientDto>.Failure("Kon de webpagina niet ophalen. Controleer de URL.");
        }

        try
        {
            var scraped = await ExtractIngredientDataAsync(pageContent, cancellationToken);
            return scraped is not null
                ? Result<ScrapedIngredientDto>.Success(scraped)
                : Result<ScrapedIngredientDto>.Failure("Kon geen ingrediëntgegevens extraheren uit de pagina.");
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "Failed to extract ingredient data from {Url}", url);
            return Result<ScrapedIngredientDto>.Failure("Er ging iets mis bij het analyseren van de pagina.");
        }
    }

    private async Task<string> FetchPageContentAsync(string url, CancellationToken cancellationToken)
    {
        using var client = httpClientFactory.CreateClient("IngredientScraper");
        client.DefaultRequestHeaders.UserAgent.ParseAdd(
            "Mozilla/5.0 (compatible; MijnKeuken/1.0)");
        client.Timeout = TimeSpan.FromSeconds(15);

        var html = await client.GetStringAsync(url, cancellationToken);

        // Strip scripts, styles, and HTML tags to reduce token usage
        html = ScriptPattern().Replace(html, " ");
        html = StylePattern().Replace(html, " ");
        html = TagPattern().Replace(html, " ");
        html = WhitespacePattern().Replace(html, " ").Trim();

        if (html.Length > MaxHtmlLength)
            html = html[..MaxHtmlLength];

        return html;
    }

    private async Task<ScrapedIngredientDto?> ExtractIngredientDataAsync(
        string pageContent,
        CancellationToken cancellationToken)
    {
        var response = await chatClient.GetResponseAsync<ScrapedIngredientDto>(
            [
                new ChatMessage(ChatRole.System, """
                    You are a product data extractor. Given the text content of a product webpage, 
                    extract the following information about the food/grocery product:
                    - Title: the product name
                    - Description: a short description of the product
                    - Amount: the total quantity (e.g. 500 for 500g, or 6 for a pack of 6 units)
                    - Unit: use "Grams" if the product is measured by weight (g, kg, ml, l), 
                      or "Units" if the product is sold by count (stuks, pieces, pack).
                      For kg, convert to grams (1kg = 1000g). For liters, convert to ml-equivalent grams.
                    Use Dutch as the output language!
                    If you cannot determine a value, use a sensible default (empty string for text, 0 for amount, Units for unit).
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