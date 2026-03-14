using MijnKeuken.Application.Interfaces;
using MijnKeuken.Application.Tags.DTOs;
using MijnKeuken.Application.Tags.Queries.GetTags;
using MijnKeuken.Domain.Entities;
using Moq;

namespace MijnKeuken.Application.Tests.Tags;

public class GetTagsHandlerTests
{
    private readonly Mock<ITagRepository> _tagRepo = new();
    private readonly GetTagsHandler _handler;

    public GetTagsHandlerTests()
    {
        _handler = new GetTagsHandler(_tagRepo.Object);
    }

    // Should return all tags mapped to DTOs
    [Fact]
    public async Task GetTags_ReturnsAllTags()
    {
        var tags = new List<Tag>
        {
            new() { Id = Guid.NewGuid(), Name = "Tomaat", Type = TagType.Ingredient, Color = "#ff0000" },
            new() { Id = Guid.NewGuid(), Name = "Diner", Type = TagType.Meal, Color = "#00ff00" },
        };

        _tagRepo.Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(tags);

        var result = await _handler.Handle(new GetTagsQuery(), CancellationToken.None);

        Assert.Equal(2, result.Count);
        Assert.Equal("Tomaat", result[0].Name);
        Assert.Equal(TagType.Ingredient, result[0].Type);
        Assert.Equal("Diner", result[1].Name);
        Assert.Equal(TagType.Meal, result[1].Type);
    }

    // Should return an empty list when no tags exist
    [Fact]
    public async Task GetTags_EmptyRepository_ReturnsEmptyList()
    {
        _tagRepo.Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync([]);

        var result = await _handler.Handle(new GetTagsQuery(), CancellationToken.None);

        Assert.Empty(result);
    }

    // DTOs should contain correct IDs and colors
    [Fact]
    public async Task GetTags_MapsAllFields()
    {
        var id = Guid.NewGuid();
        var tags = new List<Tag>
        {
            new() { Id = id, Name = "Soep", Type = TagType.Recipe, Color = "#123456" }
        };

        _tagRepo.Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(tags);

        var result = await _handler.Handle(new GetTagsQuery(), CancellationToken.None);

        Assert.Single(result);
        Assert.Equal(id, result[0].Id);
        Assert.Equal("Soep", result[0].Name);
        Assert.Equal(TagType.Recipe, result[0].Type);
        Assert.Equal("#123456", result[0].Color);
    }
}
