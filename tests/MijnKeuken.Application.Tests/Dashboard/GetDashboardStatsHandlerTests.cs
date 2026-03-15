using MijnKeuken.Application.Dashboard.DTOs;
using MijnKeuken.Application.Dashboard.Queries.GetDashboardStats;
using MijnKeuken.Application.Interfaces;
using Moq;

namespace MijnKeuken.Application.Tests.Dashboard;

public class GetDashboardStatsHandlerTests
{
    private readonly Mock<IMenuEntryRepository> _repoMock = new();
    private readonly GetDashboardStatsHandler _handler;

    public GetDashboardStatsHandlerTests()
    {
        _handler = new GetDashboardStatsHandler(_repoMock.Object);
    }

    [Fact]
    public async Task Handle_ReturnsTopRecipesAndIngredients()
    {
        var recipes = new List<RecipeUsageStatDto>
        {
            new(Guid.NewGuid(), "Pasta Bolognese", 10),
            new(Guid.NewGuid(), "Chicken Curry", 7)
        };
        var ingredients = new List<IngredientUsageStatDto>
        {
            new(Guid.NewGuid(), "Tomato", 15),
            new(Guid.NewGuid(), "Onion", 12)
        };

        _repoMock.Setup(r => r.GetTopScheduledRecipesAsync(5, It.IsAny<CancellationToken>()))
            .ReturnsAsync(recipes);
        _repoMock.Setup(r => r.GetTopUsedIngredientsAsync(5, It.IsAny<CancellationToken>()))
            .ReturnsAsync(ingredients);

        var result = await _handler.Handle(new GetDashboardStatsQuery(5), CancellationToken.None);

        Assert.Equal(2, result.TopRecipes.Count);
        Assert.Equal("Pasta Bolognese", result.TopRecipes[0].RecipeTitle);
        Assert.Equal(10, result.TopRecipes[0].ScheduleCount);
        Assert.Equal(2, result.TopIngredients.Count);
        Assert.Equal("Tomato", result.TopIngredients[0].IngredientName);
        Assert.Equal(15, result.TopIngredients[0].UsageCount);
    }

    [Fact]
    public async Task Handle_PassesTopCountToRepository()
    {
        _repoMock.Setup(r => r.GetTopScheduledRecipesAsync(3, It.IsAny<CancellationToken>()))
            .ReturnsAsync([]);
        _repoMock.Setup(r => r.GetTopUsedIngredientsAsync(3, It.IsAny<CancellationToken>()))
            .ReturnsAsync([]);

        await _handler.Handle(new GetDashboardStatsQuery(3), CancellationToken.None);

        _repoMock.Verify(r => r.GetTopScheduledRecipesAsync(3, It.IsAny<CancellationToken>()), Times.Once);
        _repoMock.Verify(r => r.GetTopUsedIngredientsAsync(3, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_EmptyData_ReturnsEmptyLists()
    {
        _repoMock.Setup(r => r.GetTopScheduledRecipesAsync(5, It.IsAny<CancellationToken>()))
            .ReturnsAsync([]);
        _repoMock.Setup(r => r.GetTopUsedIngredientsAsync(5, It.IsAny<CancellationToken>()))
            .ReturnsAsync([]);

        var result = await _handler.Handle(new GetDashboardStatsQuery(), CancellationToken.None);

        Assert.Empty(result.TopRecipes);
        Assert.Empty(result.TopIngredients);
        Assert.Null(result.NextDelivery);
    }

    [Fact]
    public async Task Handle_WithNextDelivery_ReturnsDeliveryInfo()
    {
        var delivery = new NextDeliveryDto(new DateOnly(2026, 3, 20), "Albert Heijn");

        _repoMock.Setup(r => r.GetTopScheduledRecipesAsync(5, It.IsAny<CancellationToken>()))
            .ReturnsAsync([]);
        _repoMock.Setup(r => r.GetTopUsedIngredientsAsync(5, It.IsAny<CancellationToken>()))
            .ReturnsAsync([]);
        _repoMock.Setup(r => r.GetNextDeliveryAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(delivery);

        var result = await _handler.Handle(new GetDashboardStatsQuery(), CancellationToken.None);

        Assert.NotNull(result.NextDelivery);
        Assert.Equal(new DateOnly(2026, 3, 20), result.NextDelivery.Date);
        Assert.Equal("Albert Heijn", result.NextDelivery.Note);
    }

    [Fact]
    public async Task Handle_DefaultTopCount_IsFive()
    {
        _repoMock.Setup(r => r.GetTopScheduledRecipesAsync(5, It.IsAny<CancellationToken>()))
            .ReturnsAsync([]);
        _repoMock.Setup(r => r.GetTopUsedIngredientsAsync(5, It.IsAny<CancellationToken>()))
            .ReturnsAsync([]);

        var query = new GetDashboardStatsQuery();
        await _handler.Handle(query, CancellationToken.None);

        _repoMock.Verify(r => r.GetTopScheduledRecipesAsync(5, It.IsAny<CancellationToken>()), Times.Once);
    }
}
