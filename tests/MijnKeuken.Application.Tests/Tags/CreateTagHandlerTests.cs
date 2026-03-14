using MijnKeuken.Application.Interfaces;
using MijnKeuken.Application.Tags.Commands.CreateTag;
using MijnKeuken.Domain.Entities;
using Moq;

namespace MijnKeuken.Application.Tests.Tags;

public class CreateTagHandlerTests
{
    private readonly Mock<ITagRepository> _tagRepo = new();
    private readonly CreateTagHandler _handler;

    public CreateTagHandlerTests()
    {
        _handler = new CreateTagHandler(_tagRepo.Object);
    }

    // Creating a valid tag should succeed and return the new tag's ID
    [Fact]
    public async Task CreateTag_WithValidData_ShouldSucceed()
    {
        _tagRepo.Setup(r => r.ExistsByNameAndTypeAsync(It.IsAny<string>(), It.IsAny<TagType>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        var result = await _handler.Handle(
            new CreateTagCommand("Tomaat", TagType.Ingredient, "#ff0000"), CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.NotEqual(Guid.Empty, result.Value);
        _tagRepo.Verify(r => r.AddAsync(
            It.Is<Tag>(t => t.Name == "Tomaat" && t.Type == TagType.Ingredient && t.Color == "#ff0000"),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    // An empty name should be rejected
    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData(null)]
    public async Task CreateTag_WithEmptyName_ShouldFail(string? name)
    {
        var result = await _handler.Handle(
            new CreateTagCommand(name!, TagType.Ingredient, "#ff0000"), CancellationToken.None);

        Assert.False(result.IsSuccess);
        Assert.Contains("verplicht", result.Error!, StringComparison.OrdinalIgnoreCase);
        _tagRepo.Verify(r => r.AddAsync(It.IsAny<Tag>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    // A duplicate name+type combination should be rejected
    [Fact]
    public async Task CreateTag_DuplicateNameAndType_ShouldFail()
    {
        _tagRepo.Setup(r => r.ExistsByNameAndTypeAsync("Tomaat", TagType.Ingredient, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        var result = await _handler.Handle(
            new CreateTagCommand("Tomaat", TagType.Ingredient, "#ff0000"), CancellationToken.None);

        Assert.False(result.IsSuccess);
        Assert.Contains("bestaat al", result.Error!, StringComparison.OrdinalIgnoreCase);
        _tagRepo.Verify(r => r.AddAsync(It.IsAny<Tag>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    // The same name with a different type should be allowed
    [Fact]
    public async Task CreateTag_SameNameDifferentType_ShouldSucceed()
    {
        _tagRepo.Setup(r => r.ExistsByNameAndTypeAsync("Pasta", TagType.Recipe, It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        var result = await _handler.Handle(
            new CreateTagCommand("Pasta", TagType.Recipe, "#00ff00"), CancellationToken.None);

        Assert.True(result.IsSuccess);
        _tagRepo.Verify(r => r.AddAsync(
            It.Is<Tag>(t => t.Name == "Pasta" && t.Type == TagType.Recipe),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    // Leading and trailing whitespace in the name should be trimmed
    [Fact]
    public async Task CreateTag_ShouldTrimName()
    {
        _tagRepo.Setup(r => r.ExistsByNameAndTypeAsync("Kaas", It.IsAny<TagType>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        var result = await _handler.Handle(
            new CreateTagCommand("  Kaas  ", TagType.Ingredient, "#ffcc00"), CancellationToken.None);

        Assert.True(result.IsSuccess);
        _tagRepo.Verify(r => r.AddAsync(
            It.Is<Tag>(t => t.Name == "Kaas"),
            It.IsAny<CancellationToken>()), Times.Once);
    }
}
