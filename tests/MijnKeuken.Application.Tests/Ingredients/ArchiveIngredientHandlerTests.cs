using MijnKeuken.Application.Ingredients.Commands.ArchiveIngredient;
using MijnKeuken.Application.Interfaces;
using MijnKeuken.Domain.Entities;
using Moq;

namespace MijnKeuken.Application.Tests.Ingredients;

public class ArchiveIngredientHandlerTests
{
    private readonly Mock<IIngredientRepository> _repo = new();
    private readonly ArchiveIngredientHandler _handler;

    public ArchiveIngredientHandlerTests()
    {
        _handler = new ArchiveIngredientHandler(_repo.Object);
    }

    [Fact]
    public async Task ArchiveIngredient_ExistingIngredient_ShouldSucceed()
    {
        var id = Guid.NewGuid();
        var existing = new Ingredient { Id = id, Title = "Tomaat", IsArchived = false };
        _repo.Setup(r => r.GetByIdAsync(id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(existing);

        var result = await _handler.Handle(new ArchiveIngredientCommand(id), CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.True(existing.IsArchived);
        _repo.Verify(r => r.UpdateAsync(existing, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task ArchiveIngredient_NonExistent_ShouldFail()
    {
        _repo.Setup(r => r.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Ingredient?)null);

        var result = await _handler.Handle(new ArchiveIngredientCommand(Guid.NewGuid()), CancellationToken.None);

        Assert.False(result.IsSuccess);
        Assert.Contains("niet gevonden", result.Error!, StringComparison.OrdinalIgnoreCase);
    }
}
