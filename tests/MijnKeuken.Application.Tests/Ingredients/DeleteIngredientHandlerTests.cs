using MijnKeuken.Application.Ingredients.Commands.DeleteIngredient;
using MijnKeuken.Application.Interfaces;
using MijnKeuken.Domain.Entities;
using Moq;

namespace MijnKeuken.Application.Tests.Ingredients;

public class DeleteIngredientHandlerTests
{
    private readonly Mock<IIngredientRepository> _repo = new();
    private readonly DeleteIngredientHandler _handler;

    public DeleteIngredientHandlerTests()
    {
        _handler = new DeleteIngredientHandler(_repo.Object);
    }

    [Fact]
    public async Task DeleteIngredient_ExistingIngredient_ShouldSucceed()
    {
        var id = Guid.NewGuid();
        var existing = new Ingredient { Id = id, Title = "Tomaat" };
        _repo.Setup(r => r.GetByIdAsync(id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(existing);

        var result = await _handler.Handle(new DeleteIngredientCommand(id), CancellationToken.None);

        Assert.True(result.IsSuccess);
        _repo.Verify(r => r.DeleteAsync(existing, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task DeleteIngredient_NonExistent_ShouldFail()
    {
        _repo.Setup(r => r.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Ingredient?)null);

        var result = await _handler.Handle(new DeleteIngredientCommand(Guid.NewGuid()), CancellationToken.None);

        Assert.False(result.IsSuccess);
        Assert.Contains("niet gevonden", result.Error!, StringComparison.OrdinalIgnoreCase);
    }
}
