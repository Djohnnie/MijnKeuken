using MijnKeuken.Application.Common;
using MijnKeuken.Application.Interfaces;
using MijnKeuken.Application.Recipes.DTOs;
using MijnKeuken.Application.Recipes.Queries.ScrapeRecipeFromImage;
using MijnKeuken.Domain.Entities;
using Moq;

namespace MijnKeuken.Application.Tests.Recipes;

public class ScrapeRecipeFromImageHandlerTests
{
    private readonly Mock<IRecipeImageScraperService> _imageScraperService = new();
    private readonly Mock<IIngredientRepository> _ingredientRepo = new();
    private readonly Mock<IIngredientMatchingService> _matchingService = new();
    private readonly ScrapeRecipeFromImageHandler _handler;

    public ScrapeRecipeFromImageHandlerTests()
    {
        _handler = new ScrapeRecipeFromImageHandler(
            _imageScraperService.Object,
            _ingredientRepo.Object,
            _matchingService.Object);
    }

    [Fact]
    public async Task ScrapeRecipeFromImage_WithValidJpeg_ShouldReturnScrapedRecipe()
    {
        var imageData = new byte[] { 0xFF, 0xD8, 0xFF };
        var scraped = new ScrapedRecipeDto("Pannenkoeken", "Lekker!", "## Stap 1\nMeng", 4, []);

        _imageScraperService.Setup(s => s.ScrapeAsync(imageData, "image/jpeg", It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<ScrapedRecipeDto>.Success(scraped));
        _ingredientRepo.Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync([]);

        var result = await _handler.Handle(
            new ScrapeRecipeFromImageQuery(imageData, "image/jpeg"),
            CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal("Pannenkoeken", result.Value!.Title);
    }

    [Theory]
    [InlineData("image/jpeg")]
    [InlineData("image/png")]
    [InlineData("image/gif")]
    [InlineData("image/webp")]
    public async Task ScrapeRecipeFromImage_WithAllowedContentTypes_ShouldSucceed(string contentType)
    {
        var imageData = new byte[] { 1, 2, 3 };
        var scraped = new ScrapedRecipeDto("Recept", "", "", 2, []);

        _imageScraperService.Setup(s => s.ScrapeAsync(imageData, contentType, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<ScrapedRecipeDto>.Success(scraped));
        _ingredientRepo.Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync([]);

        var result = await _handler.Handle(
            new ScrapeRecipeFromImageQuery(imageData, contentType),
            CancellationToken.None);

        Assert.True(result.IsSuccess);
    }

    [Fact]
    public async Task ScrapeRecipeFromImage_WithEmptyImageData_ShouldFail()
    {
        var result = await _handler.Handle(
            new ScrapeRecipeFromImageQuery([], "image/jpeg"),
            CancellationToken.None);

        Assert.False(result.IsSuccess);
        Assert.Contains("afbeelding", result.Error!, StringComparison.OrdinalIgnoreCase);
        _imageScraperService.Verify(s => s.ScrapeAsync(
            It.IsAny<byte[]>(), It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Theory]
    [InlineData("image/bmp")]
    [InlineData("application/pdf")]
    [InlineData("text/plain")]
    [InlineData("video/mp4")]
    public async Task ScrapeRecipeFromImage_WithInvalidContentType_ShouldFail(string contentType)
    {
        var result = await _handler.Handle(
            new ScrapeRecipeFromImageQuery(new byte[] { 1, 2, 3 }, contentType),
            CancellationToken.None);

        Assert.False(result.IsSuccess);
        Assert.Contains("bestandstype", result.Error!, StringComparison.OrdinalIgnoreCase);
        _imageScraperService.Verify(s => s.ScrapeAsync(
            It.IsAny<byte[]>(), It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task ScrapeRecipeFromImage_WhenScraperFails_ShouldReturnFailure()
    {
        _imageScraperService.Setup(s => s.ScrapeAsync(
                It.IsAny<byte[]>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<ScrapedRecipeDto>.Failure("Analyse mislukt."));

        var result = await _handler.Handle(
            new ScrapeRecipeFromImageQuery(new byte[] { 1, 2, 3 }, "image/png"),
            CancellationToken.None);

        Assert.False(result.IsSuccess);
        Assert.Equal("Analyse mislukt.", result.Error);
    }

    [Fact]
    public async Task ScrapeRecipeFromImage_NoStoredIngredients_ShouldReturnScrapedRecipeWithoutMatching()
    {
        var scraped = new ScrapedRecipeDto(
            "Pasta",
            "",
            "",
            2,
            [new ScrapedRecipeIngredientDto("Spaghetti", 500, "Grams")]);

        _imageScraperService.Setup(s => s.ScrapeAsync(
                It.IsAny<byte[]>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<ScrapedRecipeDto>.Success(scraped));
        _ingredientRepo.Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync([]);

        var result = await _handler.Handle(
            new ScrapeRecipeFromImageQuery(new byte[] { 1, 2, 3 }, "image/jpeg"),
            CancellationToken.None);

        Assert.True(result.IsSuccess);
        _matchingService.Verify(m => m.MatchAsync(
            It.IsAny<List<string>>(),
            It.IsAny<List<Ingredient>>(),
            It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task ScrapeRecipeFromImage_NoScrapedIngredients_ShouldReturnScrapedRecipeWithoutMatching()
    {
        var scraped = new ScrapedRecipeDto("Recept", "", "", 2, []);

        _imageScraperService.Setup(s => s.ScrapeAsync(
                It.IsAny<byte[]>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<ScrapedRecipeDto>.Success(scraped));
        _ingredientRepo.Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync([new Ingredient { Id = Guid.NewGuid(), Title = "Spaghetti" }]);

        var result = await _handler.Handle(
            new ScrapeRecipeFromImageQuery(new byte[] { 1, 2, 3 }, "image/png"),
            CancellationToken.None);

        Assert.True(result.IsSuccess);
        _matchingService.Verify(m => m.MatchAsync(
            It.IsAny<List<string>>(),
            It.IsAny<List<Ingredient>>(),
            It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task ScrapeRecipeFromImage_WithMatchingIngredients_ShouldEnrichIngredients()
    {
        var ingredientId = Guid.NewGuid();
        var storedIngredient = new Ingredient { Id = ingredientId, Title = "Bloem" };
        var scraped = new ScrapedRecipeDto(
            "Pannenkoeken",
            "",
            "",
            4,
            [new ScrapedRecipeIngredientDto("Bloem", 250, "Grams")]);

        _imageScraperService.Setup(s => s.ScrapeAsync(
                It.IsAny<byte[]>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<ScrapedRecipeDto>.Success(scraped));
        _ingredientRepo.Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync([storedIngredient]);
        _matchingService.Setup(m => m.MatchAsync(
                It.IsAny<List<string>>(),
                It.IsAny<List<Ingredient>>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Dictionary<string, Guid> { { "Bloem", ingredientId } });

        var result = await _handler.Handle(
            new ScrapeRecipeFromImageQuery(new byte[] { 1, 2, 3 }, "image/jpeg"),
            CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Single(result.Value!.Ingredients);
        Assert.Equal(ingredientId, result.Value.Ingredients[0].MatchedIngredientId);
        Assert.Equal("Bloem", result.Value.Ingredients[0].MatchedIngredientTitle);
    }

    [Fact]
    public async Task ScrapeRecipeFromImage_WhenNoMatchesFound_ShouldReturnOriginalScrapedRecipe()
    {
        var storedIngredient = new Ingredient { Id = Guid.NewGuid(), Title = "Pasta" };
        var scraped = new ScrapedRecipeDto(
            "Recept",
            "",
            "",
            2,
            [new ScrapedRecipeIngredientDto("Bloem", 250, "Grams")]);

        _imageScraperService.Setup(s => s.ScrapeAsync(
                It.IsAny<byte[]>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<ScrapedRecipeDto>.Success(scraped));
        _ingredientRepo.Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync([storedIngredient]);
        _matchingService.Setup(m => m.MatchAsync(
                It.IsAny<List<string>>(),
                It.IsAny<List<Ingredient>>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync([]);

        var result = await _handler.Handle(
            new ScrapeRecipeFromImageQuery(new byte[] { 1, 2, 3 }, "image/webp"),
            CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Null(result.Value!.Ingredients[0].MatchedIngredientId);
    }
}
