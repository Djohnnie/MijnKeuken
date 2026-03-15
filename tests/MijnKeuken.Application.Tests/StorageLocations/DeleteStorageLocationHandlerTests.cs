using MijnKeuken.Application.Interfaces;
using MijnKeuken.Application.StorageLocations.Commands.DeleteStorageLocation;
using MijnKeuken.Domain.Entities;
using Moq;

namespace MijnKeuken.Application.Tests.StorageLocations;

public class DeleteStorageLocationHandlerTests
{
    private readonly Mock<IStorageLocationRepository> _repo = new();
    private readonly DeleteStorageLocationHandler _handler;

    public DeleteStorageLocationHandlerTests()
    {
        _handler = new DeleteStorageLocationHandler(_repo.Object);
    }

    [Fact]
    public async Task DeleteStorageLocation_ExistingLocation_ShouldSucceed()
    {
        var id = Guid.NewGuid();
        var existing = new StorageLocation { Id = id, Name = "Koelkast", Description = "" };

        _repo.Setup(r => r.GetByIdAsync(id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(existing);

        var result = await _handler.Handle(new DeleteStorageLocationCommand(id), CancellationToken.None);

        Assert.True(result.IsSuccess);
        _repo.Verify(r => r.DeleteAsync(existing, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task DeleteStorageLocation_NonExistent_ShouldFail()
    {
        _repo.Setup(r => r.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((StorageLocation?)null);

        var result = await _handler.Handle(new DeleteStorageLocationCommand(Guid.NewGuid()), CancellationToken.None);

        Assert.False(result.IsSuccess);
        Assert.Contains("niet gevonden", result.Error!, StringComparison.OrdinalIgnoreCase);
        _repo.Verify(r => r.DeleteAsync(It.IsAny<StorageLocation>(), It.IsAny<CancellationToken>()), Times.Never);
    }
}
