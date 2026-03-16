using MijnKeuken.Application.Interfaces;
using MijnKeuken.Application.Recipes.Commands.CreateRecipe;
using MijnKeuken.Application.Recipes.Commands.UpdateRecipe;
using MijnKeuken.Domain.Entities;
using Moq;

namespace MijnKeuken.Application.Tests.Recipes;

public class UpdateRecipeHandlerTests
{
    private readonly Mock<IRecipeRepository> _repo = new();
    private readonly UpdateRecipeHandler _handler;

    public UpdateRecipeHandlerTests()
    {
        _handler = new UpdateRecipeHandler(_repo.Object);
    }

    [Fact]
    public async Task UpdateRecipe_WithValidData_ShouldSucceed()
    {
        var id = Guid.NewGuid();
        var existing = new Recipe { Id = id, Title = "Pasta", RecipeTags = [], RecipeIngredients = [] };
        _repo.Setup(r => r.GetByIdAsync(id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(existing);

        var result = await _handler.Handle(
            new UpdateRecipeCommand(id, "Pasta Bolognese", "Updated", "## Plan", [], []),
            CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal("Pasta Bolognese", existing.Title);
        Assert.Equal("## Plan", existing.Plan);
        _repo.Verify(r => r.ReplaceIngredientsAsync(
            id,
            It.Is<List<RecipeIngredient>>(list => list.Count == 0),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task UpdateRecipe_NonExistent_ShouldFail()
    {
        _repo.Setup(r => r.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Recipe?)null);

        var result = await _handler.Handle(
            new UpdateRecipeCommand(Guid.NewGuid(), "Test", "", "", [], []),
            CancellationToken.None);

        Assert.False(result.IsSuccess);
        Assert.Contains("niet gevonden", result.Error!, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task UpdateRecipe_WithEmptyTitle_ShouldFail()
    {
        var result = await _handler.Handle(
            new UpdateRecipeCommand(Guid.NewGuid(), "", "", "", [], []),
            CancellationToken.None);

        Assert.False(result.IsSuccess);
        Assert.Contains("verplicht", result.Error!, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task UpdateRecipe_ShouldReplaceIngredientsAndTags()
    {
        var id = Guid.NewGuid();
        var oldTagId = Guid.NewGuid();
        var newTagId = Guid.NewGuid();
        var newIngredientId = Guid.NewGuid();
        var existing = new Recipe
        {
            Id = id, Title = "Pasta",
            RecipeTags = [new RecipeTag { RecipeId = id, TagId = oldTagId }],
            RecipeIngredients = []
        };
        _repo.Setup(r => r.GetByIdAsync(id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(existing);

        var result = await _handler.Handle(
            new UpdateRecipeCommand(id, "Pasta", "", "",
                [newTagId],
                [new RecipeIngredientInput(newIngredientId, "Tomaat", 100, UnitType.Grams, "")]),
            CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Single(existing.RecipeTags);
        Assert.Equal(newTagId, existing.RecipeTags[0].TagId);
        Assert.Single(existing.RecipeIngredients);
        Assert.Equal(newIngredientId, existing.RecipeIngredients[0].IngredientId);
        _repo.Verify(r => r.ReplaceIngredientsAsync(
            id,
            It.Is<List<RecipeIngredient>>(list => list.Count == 1 && list[0].IngredientId == newIngredientId),
            It.IsAny<CancellationToken>()), Times.Once);
    }
}
