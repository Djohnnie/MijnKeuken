using MijnKeuken.Application.Interfaces;
using MijnKeuken.Application.Recipes.Commands.DeleteRecipe;
using MijnKeuken.Domain.Entities;
using Moq;

namespace MijnKeuken.Application.Tests.Recipes;

public class DeleteRecipeHandlerTests
{
    private readonly Mock<IRecipeRepository> _repo = new();
    private readonly DeleteRecipeHandler _handler;

    public DeleteRecipeHandlerTests()
    {
        _handler = new DeleteRecipeHandler(_repo.Object);
    }

    [Fact]
    public async Task DeleteRecipe_ExistingRecipe_ShouldSucceed()
    {
        var id = Guid.NewGuid();
        var existing = new Recipe { Id = id, Title = "Pasta" };
        _repo.Setup(r => r.GetByIdAsync(id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(existing);

        var result = await _handler.Handle(new DeleteRecipeCommand(id), CancellationToken.None);

        Assert.True(result.IsSuccess);
        _repo.Verify(r => r.DeleteAsync(existing, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task DeleteRecipe_NonExistent_ShouldFail()
    {
        _repo.Setup(r => r.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Recipe?)null);

        var result = await _handler.Handle(new DeleteRecipeCommand(Guid.NewGuid()), CancellationToken.None);

        Assert.False(result.IsSuccess);
        Assert.Contains("niet gevonden", result.Error!, StringComparison.OrdinalIgnoreCase);
    }
}
