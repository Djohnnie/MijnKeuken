using MijnKeuken.Application.Interfaces;
using MijnKeuken.Application.Tags.Commands.DeleteTag;
using MijnKeuken.Domain.Entities;
using Moq;

namespace MijnKeuken.Application.Tests.Tags;

public class DeleteTagHandlerTests
{
    private readonly Mock<ITagRepository> _tagRepo = new();
    private readonly DeleteTagHandler _handler;

    public DeleteTagHandlerTests()
    {
        _handler = new DeleteTagHandler(_tagRepo.Object);
    }

    // Deleting an existing tag should succeed and remove it via the repository
    [Fact]
    public async Task DeleteTag_ExistingTag_ShouldSucceed()
    {
        var tagId = Guid.NewGuid();
        var existingTag = new Tag { Id = tagId, Name = "Tomaat", Type = TagType.Ingredient, Color = "#ff0000" };

        _tagRepo.Setup(r => r.GetByIdAsync(tagId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingTag);

        var result = await _handler.Handle(new DeleteTagCommand(tagId), CancellationToken.None);

        Assert.True(result.IsSuccess);
        _tagRepo.Verify(r => r.DeleteAsync(existingTag, It.IsAny<CancellationToken>()), Times.Once);
    }

    // Deleting a non-existent tag should fail
    [Fact]
    public async Task DeleteTag_NonExistentTag_ShouldFail()
    {
        _tagRepo.Setup(r => r.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Tag?)null);

        var result = await _handler.Handle(new DeleteTagCommand(Guid.NewGuid()), CancellationToken.None);

        Assert.False(result.IsSuccess);
        Assert.Contains("niet gevonden", result.Error!, StringComparison.OrdinalIgnoreCase);
        _tagRepo.Verify(r => r.DeleteAsync(It.IsAny<Tag>(), It.IsAny<CancellationToken>()), Times.Never);
    }
}
