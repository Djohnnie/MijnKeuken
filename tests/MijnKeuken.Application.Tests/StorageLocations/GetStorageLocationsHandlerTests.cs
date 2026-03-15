using MijnKeuken.Application.Interfaces;
using MijnKeuken.Application.StorageLocations.Queries.GetStorageLocations;
using MijnKeuken.Domain.Entities;
using Moq;

namespace MijnKeuken.Application.Tests.StorageLocations;

public class GetStorageLocationsHandlerTests
{
    private readonly Mock<IStorageLocationRepository> _repo = new();
    private readonly GetStorageLocationsHandler _handler;

    public GetStorageLocationsHandlerTests()
    {
        _handler = new GetStorageLocationsHandler(_repo.Object);
    }

    [Fact]
    public async Task GetStorageLocations_ReturnsAllLocations()
    {
        var locations = new List<StorageLocation>
        {
            new() { Id = Guid.NewGuid(), Name = "Koelkast", Description = "In de keuken" },
            new() { Id = Guid.NewGuid(), Name = "Vriezer", Description = "In de garage" },
        };

        _repo.Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(locations);

        var result = await _handler.Handle(new GetStorageLocationsQuery(), CancellationToken.None);

        Assert.Equal(2, result.Count);
        Assert.Equal("Koelkast", result[0].Name);
        Assert.Equal("In de keuken", result[0].Description);
        Assert.Equal("Vriezer", result[1].Name);
    }

    [Fact]
    public async Task GetStorageLocations_EmptyRepository_ReturnsEmptyList()
    {
        _repo.Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync([]);

        var result = await _handler.Handle(new GetStorageLocationsQuery(), CancellationToken.None);

        Assert.Empty(result);
    }

    [Fact]
    public async Task GetStorageLocations_MapsAllFields()
    {
        var id = Guid.NewGuid();
        var locations = new List<StorageLocation>
        {
            new() { Id = id, Name = "Kelder", Description = "Onder het huis" }
        };

        _repo.Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(locations);

        var result = await _handler.Handle(new GetStorageLocationsQuery(), CancellationToken.None);

        Assert.Single(result);
        Assert.Equal(id, result[0].Id);
        Assert.Equal("Kelder", result[0].Name);
        Assert.Equal("Onder het huis", result[0].Description);
    }
}
