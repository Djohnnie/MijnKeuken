using MijnKeuken.Application.Interfaces;
using MijnKeuken.Application.Recipes.Commands.ArchiveRecipe;
using MijnKeuken.Domain.Entities;
using Moq;

namespace MijnKeuken.Application.Tests.Recipes;

public class ArchiveRecipeHandlerTests
{
    private readonly Mock<IRecipeRepository> _repo = new();
    private readonly ArchiveRecipeHandler _handler;

    public ArchiveRecipeHandlerTests()
    {
        _handler = new ArchiveRecipeHandler(_repo.Object);
    }

    [Fact]
    public async Task ArchiveRecipe_ExistingRecipe_ShouldSucceed()
    {
        var id = Guid.NewGuid();
        var existing = new Recipe { Id = id, Title = "Pasta", IsArchived = false };
        _repo.Setup(r => r.GetByIdAsync(id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(existing);

        var result = await _handler.Handle(new ArchiveRecipeCommand(id), CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.True(existing.IsArchived);
        _repo.Verify(r => r.UpdateAsync(existing, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task ArchiveRecipe_NonExistent_ShouldFail()
    {
        _repo.Setup(r => r.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Recipe?)null);

        var result = await _handler.Handle(new ArchiveRecipeCommand(Guid.NewGuid()), CancellationToken.None);

        Assert.False(result.IsSuccess);
        Assert.Contains("niet gevonden", result.Error!, StringComparison.OrdinalIgnoreCase);
    }
}
