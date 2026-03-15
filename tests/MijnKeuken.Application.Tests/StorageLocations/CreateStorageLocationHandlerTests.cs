using MijnKeuken.Application.Interfaces;
using MijnKeuken.Application.StorageLocations.Commands.CreateStorageLocation;
using MijnKeuken.Domain.Entities;
using Moq;

namespace MijnKeuken.Application.Tests.StorageLocations;

public class CreateStorageLocationHandlerTests
{
    private readonly Mock<IStorageLocationRepository> _repo = new();
    private readonly CreateStorageLocationHandler _handler;

    public CreateStorageLocationHandlerTests()
    {
        _handler = new CreateStorageLocationHandler(_repo.Object);
    }

    [Fact]
    public async Task CreateStorageLocation_WithValidData_ShouldSucceed()
    {
        _repo.Setup(r => r.ExistsByNameAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        var result = await _handler.Handle(
            new CreateStorageLocationCommand("Koelkast", "De grote koelkast in de keuken"), CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.NotEqual(Guid.Empty, result.Value);
        _repo.Verify(r => r.AddAsync(
            It.Is<StorageLocation>(l => l.Name == "Koelkast" && l.Description == "De grote koelkast in de keuken"),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData(null)]
    public async Task CreateStorageLocation_WithEmptyName_ShouldFail(string? name)
    {
        var result = await _handler.Handle(
            new CreateStorageLocationCommand(name!, "Omschrijving"), CancellationToken.None);

        Assert.False(result.IsSuccess);
        Assert.Contains("verplicht", result.Error!, StringComparison.OrdinalIgnoreCase);
        _repo.Verify(r => r.AddAsync(It.IsAny<StorageLocation>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task CreateStorageLocation_DuplicateName_ShouldFail()
    {
        _repo.Setup(r => r.ExistsByNameAsync("Koelkast", It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        var result = await _handler.Handle(
            new CreateStorageLocationCommand("Koelkast", "Beschrijving"), CancellationToken.None);

        Assert.False(result.IsSuccess);
        Assert.Contains("bestaat al", result.Error!, StringComparison.OrdinalIgnoreCase);
        _repo.Verify(r => r.AddAsync(It.IsAny<StorageLocation>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task CreateStorageLocation_ShouldTrimNameAndDescription()
    {
        _repo.Setup(r => r.ExistsByNameAsync("Vriezer", It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        var result = await _handler.Handle(
            new CreateStorageLocationCommand("  Vriezer  ", "  In de garage  "), CancellationToken.None);

        Assert.True(result.IsSuccess);
        _repo.Verify(r => r.AddAsync(
            It.Is<StorageLocation>(l => l.Name == "Vriezer" && l.Description == "In de garage"),
            It.IsAny<CancellationToken>()), Times.Once);
    }
}
