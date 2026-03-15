using MijnKeuken.Application.Ingredients.Commands.UpdateIngredient;
using MijnKeuken.Application.Interfaces;
using MijnKeuken.Domain.Entities;
using Moq;

namespace MijnKeuken.Application.Tests.Ingredients;

public class UpdateIngredientHandlerTests
{
    private readonly Mock<IIngredientRepository> _repo = new();
    private readonly UpdateIngredientHandler _handler;

    public UpdateIngredientHandlerTests()
    {
        _handler = new UpdateIngredientHandler(_repo.Object);
    }

    [Fact]
    public async Task UpdateIngredient_WithValidData_ShouldSucceed()
    {
        var id = Guid.NewGuid();
        var existing = new Ingredient { Id = id, Title = "Tomaat", IngredientTags = [] };
        _repo.Setup(r => r.GetByIdAsync(id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(existing);

        var result = await _handler.Handle(
            new UpdateIngredientCommand(id, "Rode Tomaat", "Lekker", 3, 10, UnitType.Units,
                "", "1234", "https://shop.nl", false, null, []),
            CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal("Rode Tomaat", existing.Title);
        Assert.Equal("Lekker", existing.Description);
        Assert.Equal(3m, existing.AmountAvailable);
        _repo.Verify(r => r.UpdateAsync(existing, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task UpdateIngredient_NonExistent_ShouldFail()
    {
        _repo.Setup(r => r.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Ingredient?)null);

        var result = await _handler.Handle(
            new UpdateIngredientCommand(Guid.NewGuid(), "Test", "", 0, 0, UnitType.Units,
                "", "", "", false, null, []),
            CancellationToken.None);

        Assert.False(result.IsSuccess);
        Assert.Contains("niet gevonden", result.Error!, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task UpdateIngredient_WithEmptyTitle_ShouldFail()
    {
        var result = await _handler.Handle(
            new UpdateIngredientCommand(Guid.NewGuid(), "", "", 0, 0, UnitType.Units,
                "", "", "", false, null, []),
            CancellationToken.None);

        Assert.False(result.IsSuccess);
        Assert.Contains("verplicht", result.Error!, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task UpdateIngredient_ShouldReplaceTags()
    {
        var id = Guid.NewGuid();
        var oldTagId = Guid.NewGuid();
        var newTagId = Guid.NewGuid();
        var existing = new Ingredient
        {
            Id = id, Title = "Tomaat",
            IngredientTags = [new IngredientTag { IngredientId = id, TagId = oldTagId }]
        };
        _repo.Setup(r => r.GetByIdAsync(id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(existing);

        var result = await _handler.Handle(
            new UpdateIngredientCommand(id, "Tomaat", "", 0, 0, UnitType.Units,
                "", "", "", false, null, [newTagId]),
            CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Single(existing.IngredientTags);
        Assert.Equal(newTagId, existing.IngredientTags[0].TagId);
    }
}
