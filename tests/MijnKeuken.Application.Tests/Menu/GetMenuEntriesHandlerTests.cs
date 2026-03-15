using MijnKeuken.Application.Interfaces;
using MijnKeuken.Application.Menu.Queries.GetMenuEntries;
using MijnKeuken.Domain.Entities;
using Moq;

namespace MijnKeuken.Application.Tests.Menu;

public class GetMenuEntriesHandlerTests
{
    private readonly Mock<IMenuEntryRepository> _repo = new();
    private readonly GetMenuEntriesHandler _handler;

    public GetMenuEntriesHandlerTests()
    {
        _handler = new GetMenuEntriesHandler(_repo.Object);
    }

    [Fact]
    public async Task GetEntries_ReturnsMappedDtos()
    {
        var recipe = new Recipe { Id = Guid.NewGuid(), Title = "Pasta Carbonara" };
        var entries = new List<MenuEntry>
        {
            new()
            {
                Id = Guid.NewGuid(),
                Date = new DateOnly(2026, 3, 20),
                RecipeId = recipe.Id,
                Recipe = recipe,
                HasDelivery = true,
                DeliveryNote = "AH bezorging",
                IsConsumed = false,
                IsEatingOut = false
            },
            new()
            {
                Id = Guid.NewGuid(),
                Date = new DateOnly(2026, 3, 21),
                RecipeId = null,
                Recipe = null,
                HasDelivery = false,
                DeliveryNote = "",
                IsConsumed = false,
                IsEatingOut = true
            }
        };

        _repo.Setup(r => r.GetByDateRangeAsync(
                new DateOnly(2026, 3, 20), new DateOnly(2026, 3, 21), It.IsAny<CancellationToken>()))
            .ReturnsAsync(entries);

        var result = await _handler.Handle(
            new GetMenuEntriesQuery(new DateOnly(2026, 3, 20), new DateOnly(2026, 3, 21)),
            CancellationToken.None);

        Assert.Equal(2, result.Count);

        Assert.Equal(new DateOnly(2026, 3, 20), result[0].Date);
        Assert.Equal(recipe.Id, result[0].RecipeId);
        Assert.Equal("Pasta Carbonara", result[0].RecipeTitle);
        Assert.True(result[0].HasDelivery);
        Assert.Equal("AH bezorging", result[0].DeliveryNote);
        Assert.False(result[0].IsConsumed);
        Assert.False(result[0].IsEatingOut);

        Assert.Equal(new DateOnly(2026, 3, 21), result[1].Date);
        Assert.Null(result[1].RecipeId);
        Assert.Null(result[1].RecipeTitle);
        Assert.False(result[1].HasDelivery);
        Assert.True(result[1].IsEatingOut);
    }

    [Fact]
    public async Task GetEntries_EmptyRange_ReturnsEmptyList()
    {
        _repo.Setup(r => r.GetByDateRangeAsync(
                It.IsAny<DateOnly>(), It.IsAny<DateOnly>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync([]);

        var result = await _handler.Handle(
            new GetMenuEntriesQuery(new DateOnly(2026, 1, 1), new DateOnly(2026, 1, 14)),
            CancellationToken.None);

        Assert.Empty(result);
    }

    [Fact]
    public async Task GetEntries_MapsConsumedFlag()
    {
        var recipe = new Recipe { Id = Guid.NewGuid(), Title = "Soep" };
        var entries = new List<MenuEntry>
        {
            new()
            {
                Id = Guid.NewGuid(),
                Date = new DateOnly(2026, 3, 22),
                RecipeId = recipe.Id,
                Recipe = recipe,
                IsConsumed = true
            }
        };

        _repo.Setup(r => r.GetByDateRangeAsync(
                It.IsAny<DateOnly>(), It.IsAny<DateOnly>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(entries);

        var result = await _handler.Handle(
            new GetMenuEntriesQuery(new DateOnly(2026, 3, 22), new DateOnly(2026, 3, 22)),
            CancellationToken.None);

        Assert.Single(result);
        Assert.True(result[0].IsConsumed);
        Assert.Equal("Soep", result[0].RecipeTitle);
    }
}
