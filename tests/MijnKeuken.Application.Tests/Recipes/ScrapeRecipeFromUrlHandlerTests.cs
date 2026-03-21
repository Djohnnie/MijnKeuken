using MijnKeuken.Application.Common;
using MijnKeuken.Application.Interfaces;
using MijnKeuken.Application.Recipes.DTOs;
using MijnKeuken.Application.Recipes.Queries.ScrapeRecipe;
using MijnKeuken.Domain.Entities;
using Moq;

namespace MijnKeuken.Application.Tests.Recipes;

public class ScrapeRecipeFromUrlHandlerTests
{
    private readonly Mock<IRecipeScraperService> _scraperService = new();
    private readonly Mock<IIngredientRepository> _ingredientRepo = new();
    private readonly Mock<IIngredientMatchingService> _matchingService = new();
    private readonly ScrapeRecipeFromUrlHandler _handler;

    public ScrapeRecipeFromUrlHandlerTests()
    {
        _handler = new ScrapeRecipeFromUrlHandler(
            _scraperService.Object,
            _ingredientRepo.Object,
            _matchingService.Object);
    }

    [Fact]
    public async Task ScrapeRecipeFromUrl_WithValidUrl_ShouldReturnScrapedRecipe()
    {
        var scraped = new ScrapedRecipeDto(
            "Spaghetti Bolognese",
            "Klassiek Italiaans gerecht",
            "## Stap 1\nKook pasta",
            4,
            []);

        _scraperService.Setup(s => s.ScrapeAsync("https://example.com/recipe", It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<ScrapedRecipeDto>.Success(scraped));
        _ingredientRepo.Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync([]);

        var result = await _handler.Handle(
            new ScrapeRecipeFromUrlQuery("https://example.com/recipe"),
            CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal("Spaghetti Bolognese", result.Value!.Title);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData(null)]
    public async Task ScrapeRecipeFromUrl_WithEmptyUrl_ShouldFail(string? url)
    {
        var result = await _handler.Handle(
            new ScrapeRecipeFromUrlQuery(url!),
            CancellationToken.None);

        Assert.False(result.IsSuccess);
        Assert.Contains("verplicht", result.Error!, StringComparison.OrdinalIgnoreCase);
        _scraperService.Verify(s => s.ScrapeAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Theory]
    [InlineData("not-a-url")]
    [InlineData("ftp://example.com/recipe")]
    [InlineData("file:///etc/passwd")]
    public async Task ScrapeRecipeFromUrl_WithInvalidUrl_ShouldFail(string url)
    {
        var result = await _handler.Handle(
            new ScrapeRecipeFromUrlQuery(url),
            CancellationToken.None);

        Assert.False(result.IsSuccess);
        Assert.Contains("ongeldig", result.Error!, StringComparison.OrdinalIgnoreCase);
        _scraperService.Verify(s => s.ScrapeAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task ScrapeRecipeFromUrl_WhenScraperFails_ShouldReturnFailure()
    {
        _scraperService.Setup(s => s.ScrapeAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<ScrapedRecipeDto>.Failure("Scraping mislukt."));

        var result = await _handler.Handle(
            new ScrapeRecipeFromUrlQuery("https://example.com/recipe"),
            CancellationToken.None);

        Assert.False(result.IsSuccess);
        Assert.Equal("Scraping mislukt.", result.Error);
    }

    [Fact]
    public async Task ScrapeRecipeFromUrl_ShouldTrimUrl()
    {
        var scraped = new ScrapedRecipeDto("Recept", "", "", 2, []);
        _scraperService.Setup(s => s.ScrapeAsync("https://example.com/recipe", It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<ScrapedRecipeDto>.Success(scraped));
        _ingredientRepo.Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync([]);

        await _handler.Handle(
            new ScrapeRecipeFromUrlQuery("  https://example.com/recipe  "),
            CancellationToken.None);

        _scraperService.Verify(s => s.ScrapeAsync("https://example.com/recipe", It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task ScrapeRecipeFromUrl_NoStoredIngredients_ShouldReturnScrapedRecipeWithoutMatching()
    {
        var scraped = new ScrapedRecipeDto(
            "Pasta",
            "",
            "",
            2,
            [new ScrapedRecipeIngredientDto("Spaghetti", 500, "Grams")]);

        _scraperService.Setup(s => s.ScrapeAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<ScrapedRecipeDto>.Success(scraped));
        _ingredientRepo.Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync([]);

        var result = await _handler.Handle(
            new ScrapeRecipeFromUrlQuery("https://example.com/recipe"),
            CancellationToken.None);

        Assert.True(result.IsSuccess);
        _matchingService.Verify(m => m.MatchAsync(
            It.IsAny<List<string>>(),
            It.IsAny<List<Ingredient>>(),
            It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task ScrapeRecipeFromUrl_NoScrapedIngredients_ShouldReturnScrapedRecipeWithoutMatching()
    {
        var scraped = new ScrapedRecipeDto("Pasta", "", "", 2, []);

        _scraperService.Setup(s => s.ScrapeAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<ScrapedRecipeDto>.Success(scraped));
        _ingredientRepo.Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync([new Ingredient { Id = Guid.NewGuid(), Title = "Spaghetti" }]);

        var result = await _handler.Handle(
            new ScrapeRecipeFromUrlQuery("https://example.com/recipe"),
            CancellationToken.None);

        Assert.True(result.IsSuccess);
        _matchingService.Verify(m => m.MatchAsync(
            It.IsAny<List<string>>(),
            It.IsAny<List<Ingredient>>(),
            It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task ScrapeRecipeFromUrl_WithMatchingIngredients_ShouldEnrichIngredients()
    {
        var ingredientId = Guid.NewGuid();
        var storedIngredient = new Ingredient { Id = ingredientId, Title = "Spaghetti" };
        var scraped = new ScrapedRecipeDto(
            "Pasta",
            "",
            "",
            2,
            [new ScrapedRecipeIngredientDto("Spaghetti", 500, "Grams")]);

        _scraperService.Setup(s => s.ScrapeAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<ScrapedRecipeDto>.Success(scraped));
        _ingredientRepo.Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync([storedIngredient]);
        _matchingService.Setup(m => m.MatchAsync(
                It.IsAny<List<string>>(),
                It.IsAny<List<Ingredient>>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Dictionary<string, Guid> { { "Spaghetti", ingredientId } });

        var result = await _handler.Handle(
            new ScrapeRecipeFromUrlQuery("https://example.com/recipe"),
            CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Single(result.Value!.Ingredients);
        Assert.Equal(ingredientId, result.Value.Ingredients[0].MatchedIngredientId);
        Assert.Equal("Spaghetti", result.Value.Ingredients[0].MatchedIngredientTitle);
    }

    [Fact]
    public async Task ScrapeRecipeFromUrl_WhenNoMatchesFound_ShouldReturnOriginalScrapedRecipe()
    {
        var storedIngredient = new Ingredient { Id = Guid.NewGuid(), Title = "Pasta" };
        var scraped = new ScrapedRecipeDto(
            "Recept",
            "",
            "",
            2,
            [new ScrapedRecipeIngredientDto("Spaghetti", 500, "Grams")]);

        _scraperService.Setup(s => s.ScrapeAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<ScrapedRecipeDto>.Success(scraped));
        _ingredientRepo.Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync([storedIngredient]);
        _matchingService.Setup(m => m.MatchAsync(
                It.IsAny<List<string>>(),
                It.IsAny<List<Ingredient>>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync([]);

        var result = await _handler.Handle(
            new ScrapeRecipeFromUrlQuery("https://example.com/recipe"),
            CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Null(result.Value!.Ingredients[0].MatchedIngredientId);
    }
}
