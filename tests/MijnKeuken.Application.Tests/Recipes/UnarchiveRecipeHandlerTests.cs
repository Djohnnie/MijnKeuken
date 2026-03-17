using MijnKeuken.Application.Interfaces;
using MijnKeuken.Application.Recipes.Commands.UnarchiveRecipe;
using MijnKeuken.Domain.Entities;
using Moq;

namespace MijnKeuken.Application.Tests.Recipes;

public class UnarchiveRecipeHandlerTests
{
    private readonly Mock<IRecipeRepository> _repo = new();
    private readonly UnarchiveRecipeHandler _handler;

    public UnarchiveRecipeHandlerTests()
    {
        _handler = new UnarchiveRecipeHandler(_repo.Object);
    }

    [Fact]
    public async Task UnarchiveRecipe_ExistingRecipe_ShouldSucceed()
    {
        var id = Guid.NewGuid();
        var existing = new Recipe { Id = id, Title = "Pasta", IsArchived = true };
        _repo.Setup(r => r.GetByIdAsync(id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(existing);

        var result = await _handler.Handle(new UnarchiveRecipeCommand(id), CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.False(existing.IsArchived);
        _repo.Verify(r => r.UpdateAsync(existing, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task UnarchiveRecipe_NonExistent_ShouldFail()
    {
        _repo.Setup(r => r.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Recipe?)null);

        var result = await _handler.Handle(new UnarchiveRecipeCommand(Guid.NewGuid()), CancellationToken.None);

        Assert.False(result.IsSuccess);
        Assert.Contains("niet gevonden", result.Error!, StringComparison.OrdinalIgnoreCase);
    }
}
