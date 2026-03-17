using MijnKeuken.Application.Ingredients.Commands.UnarchiveIngredient;
using MijnKeuken.Application.Interfaces;
using MijnKeuken.Domain.Entities;
using Moq;

namespace MijnKeuken.Application.Tests.Ingredients;

public class UnarchiveIngredientHandlerTests
{
    private readonly Mock<IIngredientRepository> _repo = new();
    private readonly UnarchiveIngredientHandler _handler;

    public UnarchiveIngredientHandlerTests()
    {
        _handler = new UnarchiveIngredientHandler(_repo.Object);
    }

    [Fact]
    public async Task UnarchiveIngredient_ExistingIngredient_ShouldSucceed()
    {
        var id = Guid.NewGuid();
        var existing = new Ingredient { Id = id, Title = "Tomaat", IsArchived = true };
        _repo.Setup(r => r.GetByIdAsync(id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(existing);

        var result = await _handler.Handle(new UnarchiveIngredientCommand(id), CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.False(existing.IsArchived);
        _repo.Verify(r => r.UpdateAsync(existing, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task UnarchiveIngredient_NonExistent_ShouldFail()
    {
        _repo.Setup(r => r.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Ingredient?)null);

        var result = await _handler.Handle(new UnarchiveIngredientCommand(Guid.NewGuid()), CancellationToken.None);

        Assert.False(result.IsSuccess);
        Assert.Contains("niet gevonden", result.Error!, StringComparison.OrdinalIgnoreCase);
    }
}
