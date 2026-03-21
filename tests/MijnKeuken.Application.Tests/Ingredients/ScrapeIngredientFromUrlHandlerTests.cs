using MijnKeuken.Application.Common;
using MijnKeuken.Application.Ingredients.DTOs;
using MijnKeuken.Application.Ingredients.Queries.ScrapeIngredient;
using MijnKeuken.Application.Interfaces;
using MijnKeuken.Domain.Entities;
using Moq;

namespace MijnKeuken.Application.Tests.Ingredients;

public class ScrapeIngredientFromUrlHandlerTests
{
    private readonly Mock<IIngredientScraperService> _scraperService = new();
    private readonly ScrapeIngredientFromUrlHandler _handler;

    public ScrapeIngredientFromUrlHandlerTests()
    {
        _handler = new ScrapeIngredientFromUrlHandler(_scraperService.Object);
    }

    [Fact]
    public async Task ScrapeIngredientFromUrl_WithValidUrl_ShouldReturnScrapedIngredient()
    {
        var expected = new ScrapedIngredientDto("Pasta", "Droge pasta", 500, UnitType.Grams);
        _scraperService.Setup(s => s.ScrapeAsync("https://example.com/product", It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<ScrapedIngredientDto>.Success(expected));

        var result = await _handler.Handle(
            new ScrapeIngredientFromUrlQuery("https://example.com/product"),
            CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal("Pasta", result.Value!.Title);
        Assert.Equal(500, result.Value.Amount);
        Assert.Equal(UnitType.Grams, result.Value.Unit);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData(null)]
    public async Task ScrapeIngredientFromUrl_WithEmptyUrl_ShouldFail(string? url)
    {
        var result = await _handler.Handle(
            new ScrapeIngredientFromUrlQuery(url!),
            CancellationToken.None);

        Assert.False(result.IsSuccess);
        Assert.Contains("verplicht", result.Error!, StringComparison.OrdinalIgnoreCase);
        _scraperService.Verify(s => s.ScrapeAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Theory]
    [InlineData("not-a-url")]
    [InlineData("ftp://example.com/product")]
    [InlineData("javascript:alert(1)")]
    public async Task ScrapeIngredientFromUrl_WithInvalidUrl_ShouldFail(string url)
    {
        var result = await _handler.Handle(
            new ScrapeIngredientFromUrlQuery(url),
            CancellationToken.None);

        Assert.False(result.IsSuccess);
        Assert.Contains("ongeldig", result.Error!, StringComparison.OrdinalIgnoreCase);
        _scraperService.Verify(s => s.ScrapeAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task ScrapeIngredientFromUrl_WhenScraperFails_ShouldReturnFailure()
    {
        _scraperService.Setup(s => s.ScrapeAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<ScrapedIngredientDto>.Failure("Scraping mislukt."));

        var result = await _handler.Handle(
            new ScrapeIngredientFromUrlQuery("https://example.com/product"),
            CancellationToken.None);

        Assert.False(result.IsSuccess);
        Assert.Equal("Scraping mislukt.", result.Error);
    }

    [Fact]
    public async Task ScrapeIngredientFromUrl_ShouldTrimUrl()
    {
        var expected = new ScrapedIngredientDto("Melk", "Volle melk", 1, UnitType.Units);
        _scraperService.Setup(s => s.ScrapeAsync("https://example.com/product", It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<ScrapedIngredientDto>.Success(expected));

        var result = await _handler.Handle(
            new ScrapeIngredientFromUrlQuery("  https://example.com/product  "),
            CancellationToken.None);

        Assert.True(result.IsSuccess);
        _scraperService.Verify(s => s.ScrapeAsync("https://example.com/product", It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task ScrapeIngredientFromUrl_WithHttpUrl_ShouldSucceed()
    {
        var expected = new ScrapedIngredientDto("Bloem", "Tarwebloem", 1000, UnitType.Grams);
        _scraperService.Setup(s => s.ScrapeAsync("http://example.com/product", It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<ScrapedIngredientDto>.Success(expected));

        var result = await _handler.Handle(
            new ScrapeIngredientFromUrlQuery("http://example.com/product"),
            CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal("Bloem", result.Value!.Title);
    }
}
