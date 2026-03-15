using MijnKeuken.Application.Interfaces;
using MijnKeuken.Application.Recipes.Commands.CreateRecipe;
using MijnKeuken.Domain.Entities;
using Moq;

namespace MijnKeuken.Application.Tests.Recipes;

public class CreateRecipeHandlerTests
{
    private readonly Mock<IRecipeRepository> _repo = new();
    private readonly CreateRecipeHandler _handler;

    public CreateRecipeHandlerTests()
    {
        _handler = new CreateRecipeHandler(_repo.Object);
    }

    [Fact]
    public async Task CreateRecipe_WithValidData_ShouldSucceed()
    {
        _repo.Setup(r => r.ExistsByTitleAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        var result = await _handler.Handle(
            new CreateRecipeCommand("Pasta Bolognese", "Lekker recept", "## Stappen\n1. Kook pasta",
                [], []),
            CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.NotEqual(Guid.Empty, result.Value);
        _repo.Verify(r => r.AddAsync(
            It.Is<Recipe>(rec => rec.Title == "Pasta Bolognese" && rec.Plan.Contains("Stappen")),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData(null)]
    public async Task CreateRecipe_WithEmptyTitle_ShouldFail(string? title)
    {
        var result = await _handler.Handle(
            new CreateRecipeCommand(title!, "", "", [], []),
            CancellationToken.None);

        Assert.False(result.IsSuccess);
        Assert.Contains("verplicht", result.Error!, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task CreateRecipe_DuplicateTitle_ShouldFail()
    {
        _repo.Setup(r => r.ExistsByTitleAsync("Pasta", It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        var result = await _handler.Handle(
            new CreateRecipeCommand("Pasta", "", "", [], []),
            CancellationToken.None);

        Assert.False(result.IsSuccess);
        Assert.Contains("bestaat al", result.Error!, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task CreateRecipe_ShouldTrimTitle()
    {
        _repo.Setup(r => r.ExistsByTitleAsync("Soep", It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        await _handler.Handle(
            new CreateRecipeCommand("  Soep  ", "", "", [], []),
            CancellationToken.None);

        _repo.Verify(r => r.AddAsync(
            It.Is<Recipe>(rec => rec.Title == "Soep"),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task CreateRecipe_WithTagsAndIngredients_ShouldCreateAssociations()
    {
        var tagId = Guid.NewGuid();
        var ingredientId = Guid.NewGuid();
        _repo.Setup(r => r.ExistsByTitleAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        var result = await _handler.Handle(
            new CreateRecipeCommand("Salade", "", "",
                [tagId],
                [new RecipeIngredientInput(ingredientId, 200, UnitType.Grams, "")]),
            CancellationToken.None);

        Assert.True(result.IsSuccess);
        _repo.Verify(r => r.AddAsync(
            It.Is<Recipe>(rec => rec.RecipeTags.Count == 1 && rec.RecipeIngredients.Count == 1),
            It.IsAny<CancellationToken>()), Times.Once);
    }
}
