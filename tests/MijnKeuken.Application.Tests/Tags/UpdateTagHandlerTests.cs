using MijnKeuken.Application.Interfaces;
using MijnKeuken.Application.Tags.Commands.UpdateTag;
using MijnKeuken.Domain.Entities;
using Moq;

namespace MijnKeuken.Application.Tests.Tags;

public class UpdateTagHandlerTests
{
    private readonly Mock<ITagRepository> _tagRepo = new();
    private readonly UpdateTagHandler _handler;

    public UpdateTagHandlerTests()
    {
        _handler = new UpdateTagHandler(_tagRepo.Object);
    }

    // Updating an existing tag should modify its properties and succeed
    [Fact]
    public async Task UpdateTag_ExistingTag_ShouldSucceed()
    {
        var tagId = Guid.NewGuid();
        var existingTag = new Tag { Id = tagId, Name = "Tomaat", Type = TagType.Ingredient, Color = "#ff0000" };

        _tagRepo.Setup(r => r.GetByIdAsync(tagId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingTag);

        var result = await _handler.Handle(
            new UpdateTagCommand(tagId, "Tomaten", TagType.Ingredient, "#cc0000"), CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal("Tomaten", existingTag.Name);
        Assert.Equal("#cc0000", existingTag.Color);
        _tagRepo.Verify(r => r.UpdateAsync(existingTag, It.IsAny<CancellationToken>()), Times.Once);
    }

    // Updating a non-existent tag should fail
    [Fact]
    public async Task UpdateTag_NonExistentTag_ShouldFail()
    {
        _tagRepo.Setup(r => r.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Tag?)null);

        var result = await _handler.Handle(
            new UpdateTagCommand(Guid.NewGuid(), "Test", TagType.Recipe, "#000"), CancellationToken.None);

        Assert.False(result.IsSuccess);
        Assert.Contains("niet gevonden", result.Error!, StringComparison.OrdinalIgnoreCase);
        _tagRepo.Verify(r => r.UpdateAsync(It.IsAny<Tag>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    // Updating with an empty name should fail
    [Fact]
    public async Task UpdateTag_EmptyName_ShouldFail()
    {
        var result = await _handler.Handle(
            new UpdateTagCommand(Guid.NewGuid(), "", TagType.Recipe, "#000"), CancellationToken.None);

        Assert.False(result.IsSuccess);
        Assert.Contains("verplicht", result.Error!, StringComparison.OrdinalIgnoreCase);
    }

    // Updating should allow changing the tag type
    [Fact]
    public async Task UpdateTag_CanChangeType()
    {
        var tagId = Guid.NewGuid();
        var existingTag = new Tag { Id = tagId, Name = "Pasta", Type = TagType.Ingredient, Color = "#fff" };

        _tagRepo.Setup(r => r.GetByIdAsync(tagId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingTag);

        var result = await _handler.Handle(
            new UpdateTagCommand(tagId, "Pasta", TagType.Recipe, "#fff"), CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal(TagType.Recipe, existingTag.Type);
    }
}
