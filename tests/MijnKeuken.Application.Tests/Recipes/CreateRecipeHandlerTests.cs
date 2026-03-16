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
            new CreateRecipeCommand("Pasta Bolognese", "Lekker recept", "## Stappen\n1. Kook pasta", 2, "",
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
            new CreateRecipeCommand(title!, "", "", 2, "", [], []),
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
            new CreateRecipeCommand("Pasta", "", "", 2, "", [], []),
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
            new CreateRecipeCommand("  Soep  ", "", "", 2, "", [], []),
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
                2, "",
                [tagId],
                [new RecipeIngredientInput(ingredientId, "Sla", 200, UnitType.Grams, "", 0)]),
            CancellationToken.None);

        Assert.True(result.IsSuccess);
        _repo.Verify(r => r.AddAsync(
            It.Is<Recipe>(rec => rec.RecipeTags.Count == 1 && rec.RecipeIngredients.Count == 1),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(-10)]
    public async Task CreateRecipe_WithInvalidServings_ShouldFail(int servings)
    {
        var result = await _handler.Handle(
            new CreateRecipeCommand("Test", "", "", servings, "", [], []),
            CancellationToken.None);

        Assert.False(result.IsSuccess);
        Assert.Contains("personen", result.Error!, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task CreateRecipe_ShouldPersistServings()
    {
        _repo.Setup(r => r.ExistsByTitleAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        await _handler.Handle(
            new CreateRecipeCommand("Stoofvlees", "", "", 6, "", [], []),
            CancellationToken.None);

        _repo.Verify(r => r.AddAsync(
            It.Is<Recipe>(rec => rec.Servings == 6),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-5)]
    public async Task CreateRecipe_WithInvalidIngredientAmount_ShouldFail(decimal amount)
    {
        _repo.Setup(r => r.ExistsByTitleAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        var result = await _handler.Handle(
            new CreateRecipeCommand("Test", "", "", 2, "", [],
                [new RecipeIngredientInput(null, "Bloem", amount, UnitType.Grams, "", 0)]),
            CancellationToken.None);

        Assert.False(result.IsSuccess);
        Assert.Contains("Hoeveelheid", result.Error!, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task CreateRecipe_ShouldPersistIngredientSortOrder()
    {
        _repo.Setup(r => r.ExistsByTitleAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        var result = await _handler.Handle(
            new CreateRecipeCommand("Pasta", "", "", 2, "", [],
            [
                new RecipeIngredientInput(null, "Pasta", 500, UnitType.Grams, "", 0),
                new RecipeIngredientInput(null, "Saus", 200, UnitType.Grams, "", 1),
                new RecipeIngredientInput(null, "Kaas", 100, UnitType.Grams, "", 2)
            ]),
            CancellationToken.None);

        Assert.True(result.IsSuccess);
        _repo.Verify(r => r.AddAsync(
            It.Is<Recipe>(rec =>
                rec.RecipeIngredients.Count == 3 &&
                rec.RecipeIngredients[0].SortOrder == 0 &&
                rec.RecipeIngredients[1].SortOrder == 1 &&
                rec.RecipeIngredients[2].SortOrder == 2),
            It.IsAny<CancellationToken>()), Times.Once);
    }
}
