using Azure;
using Azure.AI.ContentUnderstanding;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.Logging;
using MijnKeuken.Application.Common;
using MijnKeuken.Application.Interfaces;
using MijnKeuken.Application.Recipes.DTOs;

namespace MijnKeuken.Infrastructure.Services;

public class RecipeImageScraperService(
    ContentUnderstandingClient contentUnderstandingClient,
    IChatClient chatClient,
    ILogger<RecipeImageScraperService> logger) : IRecipeImageScraperService
{
    private const int MaxImageSize = 10 * 1024 * 1024; // 10 MB

    public async Task<Result<ScrapedRecipeDto>> ScrapeAsync(
        byte[] imageData,
        string contentType,
        CancellationToken cancellationToken = default)
    {
        if (imageData.Length > MaxImageSize)
            return Result<ScrapedRecipeDto>.Failure("Afbeelding is te groot (maximaal 10 MB).");

        string ocrText;
        try
        {
            ocrText = await ExtractTextWithOcrAsync(imageData, contentType, cancellationToken);
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "OCR extraction failed");
            return Result<ScrapedRecipeDto>.Failure("Kon geen tekst uit de afbeelding halen.");
        }

        try
        {
            var scraped = await ExtractRecipeDataAsync(ocrText, imageData, contentType, cancellationToken);
            return scraped is not null
                ? Result<ScrapedRecipeDto>.Success(scraped)
                : Result<ScrapedRecipeDto>.Failure("Kon geen receptgegevens extraheren uit de afbeelding.");
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "Failed to extract recipe data from image");
            return Result<ScrapedRecipeDto>.Failure("Er ging iets mis bij het analyseren van de afbeelding.");
        }
    }

    private async Task<string> ExtractTextWithOcrAsync(
        byte[] imageData,
        string contentType,
        CancellationToken cancellationToken)
    {
        //var input = new AnalysisInput { Data = BinaryData.FromBytes(imageData), MimeType = contentType };
        var operation = await contentUnderstandingClient.AnalyzeBinaryAsync(
            WaitUntil.Completed,
            "prebuilt-read",
            BinaryData.FromBytes(imageData),
            cancellationToken: cancellationToken);

        var markdown = string.Join("\n\n",
            operation.Value.Contents
                .Where(c => !string.IsNullOrWhiteSpace(c.Markdown))
                .Select(c => c.Markdown));

        return string.IsNullOrWhiteSpace(markdown)
            ? string.Empty
            : markdown;
    }

    private async Task<ScrapedRecipeDto?> ExtractRecipeDataAsync(
        string ocrText,
        byte[] imageData,
        string contentType,
        CancellationToken cancellationToken)
    {
        var response = await chatClient.GetResponseAsync<ScrapedRecipeDto>(
            [
                new ChatMessage(ChatRole.System, """
                    You are a recipe data extractor. You will receive an image of a recipe together with
                    the OCR-extracted text from that image. Use both sources to accurately extract:
                    - Title: the recipe name
                    - Description: a short description of the dish
                    - Plan: the full cooking instructions formatted as Markdown, extracted from the OCR text and structured like the original with headings if in the original.
                    - Servings: the number of servings, or null if not specified
                    - Ingredients: a list of ingredients, each with:
                      - Name: ingredient name
                      - Amount: numeric quantity (e.g. 500 for 500g, 2 for 2 pieces, 1 if unspecified)
                      - Unit: the unit as text (e.g. "gram", "stuks", "el", "tl", "ml", "snuf", "bos", "teen")
                      - If ingredients are mentioned multiple times in the OCR text, combine them into one ingredient with the total amount (e.g. "2 el olijfolie" and "1 el olijfolie" becomes "3 el olijfolie").
                    Use Dutch as the output language!
                    Start every cooking instruction step and ingredient name with an uppercase letter.
                    Prefer the OCR text for exact ingredient names and quantities.
                    Use the image to resolve ambiguities or missing information from the OCR.
                    """),
                new ChatMessage(ChatRole.User,
                [
                    new TextContent($"""
                        Haal het recept uit deze OCR-tekst uit de afbeelding:
                        ---
                        {ocrText}
                        ---
                        """),
                    new DataContent(imageData, contentType)
                ])
            ],
            cancellationToken: cancellationToken);

        return response.Result;
    }
}