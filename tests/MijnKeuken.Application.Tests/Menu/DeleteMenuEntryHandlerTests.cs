using MijnKeuken.Application.Interfaces;
using MijnKeuken.Application.Menu.Commands.DeleteMenuEntry;
using MijnKeuken.Domain.Entities;
using Moq;

namespace MijnKeuken.Application.Tests.Menu;

public class DeleteMenuEntryHandlerTests
{
    private readonly Mock<IMenuEntryRepository> _repo = new();
    private readonly DeleteMenuEntryHandler _handler;

    public DeleteMenuEntryHandlerTests()
    {
        _handler = new DeleteMenuEntryHandler(_repo.Object);
    }

    [Fact]
    public async Task Delete_ExistingEntry_ShouldSucceed()
    {
        var entryId = Guid.NewGuid();
        var existing = new MenuEntry { Id = entryId, Date = new DateOnly(2026, 3, 20) };

        _repo.Setup(r => r.GetByIdAsync(entryId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(existing);

        var result = await _handler.Handle(new DeleteMenuEntryCommand(entryId), CancellationToken.None);

        Assert.True(result.IsSuccess);
        _repo.Verify(r => r.DeleteAsync(existing, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Delete_NonExistentEntry_ShouldFail()
    {
        _repo.Setup(r => r.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((MenuEntry?)null);

        var result = await _handler.Handle(new DeleteMenuEntryCommand(Guid.NewGuid()), CancellationToken.None);

        Assert.False(result.IsSuccess);
        Assert.Contains("niet gevonden", result.Error!, StringComparison.OrdinalIgnoreCase);
        _repo.Verify(r => r.DeleteAsync(It.IsAny<MenuEntry>(), It.IsAny<CancellationToken>()), Times.Never);
    }
}
