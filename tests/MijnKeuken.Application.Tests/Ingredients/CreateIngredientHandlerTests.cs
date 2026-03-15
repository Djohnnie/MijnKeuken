using MijnKeuken.Application.Ingredients.Commands.CreateIngredient;
using MijnKeuken.Application.Interfaces;
using MijnKeuken.Domain.Entities;
using Moq;

namespace MijnKeuken.Application.Tests.Ingredients;

public class CreateIngredientHandlerTests
{
    private readonly Mock<IIngredientRepository> _repo = new();
    private readonly CreateIngredientHandler _handler;

    public CreateIngredientHandlerTests()
    {
        _handler = new CreateIngredientHandler(_repo.Object);
    }

    [Fact]
    public async Task CreateIngredient_WithValidData_ShouldSucceed()
    {
        _repo.Setup(r => r.ExistsByTitleAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        var result = await _handler.Handle(
            new CreateIngredientCommand("Tomaat", "Rode tomaat", 5, 10, UnitType.Units,
                "", "", "", false, null, []),
            CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.NotEqual(Guid.Empty, result.Value);
        _repo.Verify(r => r.AddAsync(
            It.Is<Ingredient>(i => i.Title == "Tomaat" && i.Description == "Rode tomaat"),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData(null)]
    public async Task CreateIngredient_WithEmptyTitle_ShouldFail(string? title)
    {
        var result = await _handler.Handle(
            new CreateIngredientCommand(title!, "", 0, 0, UnitType.Units, "", "", "", false, null, []),
            CancellationToken.None);

        Assert.False(result.IsSuccess);
        Assert.Contains("verplicht", result.Error!, StringComparison.OrdinalIgnoreCase);
        _repo.Verify(r => r.AddAsync(It.IsAny<Ingredient>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task CreateIngredient_DuplicateTitle_ShouldFail()
    {
        _repo.Setup(r => r.ExistsByTitleAsync("Tomaat", It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        var result = await _handler.Handle(
            new CreateIngredientCommand("Tomaat", "", 0, 0, UnitType.Units, "", "", "", false, null, []),
            CancellationToken.None);

        Assert.False(result.IsSuccess);
        Assert.Contains("bestaat al", result.Error!, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task CreateIngredient_ShouldTrimTitle()
    {
        _repo.Setup(r => r.ExistsByTitleAsync("Kaas", It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        var result = await _handler.Handle(
            new CreateIngredientCommand("  Kaas  ", "", 0, 0, UnitType.Grams, "", "", "", false, null, []),
            CancellationToken.None);

        Assert.True(result.IsSuccess);
        _repo.Verify(r => r.AddAsync(
            It.Is<Ingredient>(i => i.Title == "Kaas"),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task CreateIngredient_WithTags_ShouldCreateAssociations()
    {
        var tagId1 = Guid.NewGuid();
        var tagId2 = Guid.NewGuid();
        _repo.Setup(r => r.ExistsByTitleAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        var result = await _handler.Handle(
            new CreateIngredientCommand("Pasta", "", 0, 0, UnitType.Grams, "", "", "", false, null,
                [tagId1, tagId2]),
            CancellationToken.None);

        Assert.True(result.IsSuccess);
        _repo.Verify(r => r.AddAsync(
            It.Is<Ingredient>(i => i.IngredientTags.Count == 2),
            It.IsAny<CancellationToken>()), Times.Once);
    }
}
