using MijnKeuken.Application.Archive.DTOs;
using MijnKeuken.Application.Archive.Queries.GetArchivedItems;
using MijnKeuken.Application.Interfaces;
using MijnKeuken.Domain.Entities;
using Moq;

namespace MijnKeuken.Application.Tests.Archive;

public class GetArchivedItemsHandlerTests
{
    private readonly Mock<IRecipeRepository> _recipeRepo = new();
    private readonly Mock<IIngredientRepository> _ingredientRepo = new();
    private readonly GetArchivedItemsHandler _handler;

    public GetArchivedItemsHandlerTests()
    {
        _handler = new GetArchivedItemsHandler(_recipeRepo.Object, _ingredientRepo.Object);
    }

    [Fact]
    public async Task GetArchivedItems_ShouldReturnBothTypes()
    {
        _recipeRepo.Setup(r => r.GetArchivedAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync([new Recipe { Id = Guid.NewGuid(), Title = "Pasta", IsArchived = true }]);
        _ingredientRepo.Setup(r => r.GetArchivedAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync([new Ingredient { Id = Guid.NewGuid(), Title = "Tomaat", IsArchived = true }]);

        var result = await _handler.Handle(new GetArchivedItemsQuery(), CancellationToken.None);

        Assert.Equal(2, result.Count);
        Assert.Contains(result, i => i.Title == "Pasta" && i.Type == ArchivedItemType.Recipe);
        Assert.Contains(result, i => i.Title == "Tomaat" && i.Type == ArchivedItemType.Ingredient);
    }

    [Fact]
    public async Task GetArchivedItems_ShouldReturnSortedByTitle()
    {
        _recipeRepo.Setup(r => r.GetArchivedAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync([new Recipe { Id = Guid.NewGuid(), Title = "Zuurkool", IsArchived = true }]);
        _ingredientRepo.Setup(r => r.GetArchivedAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync([new Ingredient { Id = Guid.NewGuid(), Title = "Aardappel", IsArchived = true }]);

        var result = await _handler.Handle(new GetArchivedItemsQuery(), CancellationToken.None);

        Assert.Equal("Aardappel", result[0].Title);
        Assert.Equal("Zuurkool", result[1].Title);
    }

    [Fact]
    public async Task GetArchivedItems_Empty_ShouldReturnEmpty()
    {
        _recipeRepo.Setup(r => r.GetArchivedAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync([]);
        _ingredientRepo.Setup(r => r.GetArchivedAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync([]);

        var result = await _handler.Handle(new GetArchivedItemsQuery(), CancellationToken.None);

        Assert.Empty(result);
    }

    [Fact]
    public async Task GetArchivedItems_OnlyRecipes_ShouldReturnRecipesOnly()
    {
        _recipeRepo.Setup(r => r.GetArchivedAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync([
                new Recipe { Id = Guid.NewGuid(), Title = "Soep", IsArchived = true },
                new Recipe { Id = Guid.NewGuid(), Title = "Pasta", IsArchived = true }
            ]);
        _ingredientRepo.Setup(r => r.GetArchivedAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync([]);

        var result = await _handler.Handle(new GetArchivedItemsQuery(), CancellationToken.None);

        Assert.Equal(2, result.Count);
        Assert.All(result, i => Assert.Equal(ArchivedItemType.Recipe, i.Type));
    }
}
