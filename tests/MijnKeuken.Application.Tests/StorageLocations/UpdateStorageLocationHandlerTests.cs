using MijnKeuken.Application.Interfaces;
using MijnKeuken.Application.StorageLocations.Commands.UpdateStorageLocation;
using MijnKeuken.Domain.Entities;
using Moq;

namespace MijnKeuken.Application.Tests.StorageLocations;

public class UpdateStorageLocationHandlerTests
{
    private readonly Mock<IStorageLocationRepository> _repo = new();
    private readonly UpdateStorageLocationHandler _handler;

    public UpdateStorageLocationHandlerTests()
    {
        _handler = new UpdateStorageLocationHandler(_repo.Object);
    }

    [Fact]
    public async Task UpdateStorageLocation_ExistingLocation_ShouldSucceed()
    {
        var id = Guid.NewGuid();
        var existing = new StorageLocation { Id = id, Name = "Koelkast", Description = "Oud" };

        _repo.Setup(r => r.GetByIdAsync(id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(existing);

        var result = await _handler.Handle(
            new UpdateStorageLocationCommand(id, "Vriezer", "Nieuw"), CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal("Vriezer", existing.Name);
        Assert.Equal("Nieuw", existing.Description);
        _repo.Verify(r => r.UpdateAsync(existing, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task UpdateStorageLocation_NonExistent_ShouldFail()
    {
        _repo.Setup(r => r.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((StorageLocation?)null);

        var result = await _handler.Handle(
            new UpdateStorageLocationCommand(Guid.NewGuid(), "Test", "Desc"), CancellationToken.None);

        Assert.False(result.IsSuccess);
        Assert.Contains("niet gevonden", result.Error!, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task UpdateStorageLocation_EmptyName_ShouldFail()
    {
        var result = await _handler.Handle(
            new UpdateStorageLocationCommand(Guid.NewGuid(), "", "Desc"), CancellationToken.None);

        Assert.False(result.IsSuccess);
        Assert.Contains("verplicht", result.Error!, StringComparison.OrdinalIgnoreCase);
    }
}
