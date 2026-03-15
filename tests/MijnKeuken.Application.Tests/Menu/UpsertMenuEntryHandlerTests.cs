using MijnKeuken.Application.Interfaces;
using MijnKeuken.Application.Menu.Commands.UpsertMenuEntry;
using MijnKeuken.Domain.Entities;
using Moq;

namespace MijnKeuken.Application.Tests.Menu;

public class UpsertMenuEntryHandlerTests
{
    private readonly Mock<IMenuEntryRepository> _repo = new();
    private readonly UpsertMenuEntryHandler _handler;

    public UpsertMenuEntryHandlerTests()
    {
        _handler = new UpsertMenuEntryHandler(_repo.Object);
    }

    [Fact]
    public async Task Upsert_NewEntryWithRecipe_ShouldCreate()
    {
        var date = new DateOnly(2026, 3, 20);
        var recipeId = Guid.NewGuid();

        _repo.Setup(r => r.GetByDateAsync(date, It.IsAny<CancellationToken>()))
            .ReturnsAsync((MenuEntry?)null);

        var result = await _handler.Handle(
            new UpsertMenuEntryCommand(date, recipeId, false, "", false, false), CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.NotEqual(Guid.Empty, result.Value);
        _repo.Verify(r => r.AddAsync(
            It.Is<MenuEntry>(e => e.Date == date && e.RecipeId == recipeId),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Upsert_ExistingEntry_ShouldUpdate()
    {
        var date = new DateOnly(2026, 3, 20);
        var existingId = Guid.NewGuid();
        var newRecipeId = Guid.NewGuid();
        var existing = new MenuEntry { Id = existingId, Date = date, RecipeId = Guid.NewGuid() };

        _repo.Setup(r => r.GetByDateAsync(date, It.IsAny<CancellationToken>()))
            .ReturnsAsync(existing);

        var result = await _handler.Handle(
            new UpsertMenuEntryCommand(date, newRecipeId, true, "AH bezorging", false, false), CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal(existingId, result.Value);
        Assert.Equal(newRecipeId, existing.RecipeId);
        Assert.True(existing.HasDelivery);
        Assert.Equal("AH bezorging", existing.DeliveryNote);
        _repo.Verify(r => r.UpdateAsync(existing, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Upsert_EmptyEntryWithExisting_ShouldDelete()
    {
        var date = new DateOnly(2026, 3, 20);
        var existing = new MenuEntry { Id = Guid.NewGuid(), Date = date, RecipeId = Guid.NewGuid() };

        _repo.Setup(r => r.GetByDateAsync(date, It.IsAny<CancellationToken>()))
            .ReturnsAsync(existing);

        var result = await _handler.Handle(
            new UpsertMenuEntryCommand(date, null, false, "", false, false), CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal(Guid.Empty, result.Value);
        _repo.Verify(r => r.DeleteAsync(existing, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Upsert_EmptyEntryNoExisting_ShouldReturnEmptyGuid()
    {
        var date = new DateOnly(2026, 3, 20);

        _repo.Setup(r => r.GetByDateAsync(date, It.IsAny<CancellationToken>()))
            .ReturnsAsync((MenuEntry?)null);

        var result = await _handler.Handle(
            new UpsertMenuEntryCommand(date, null, false, "", false, false), CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal(Guid.Empty, result.Value);
        _repo.Verify(r => r.AddAsync(It.IsAny<MenuEntry>(), It.IsAny<CancellationToken>()), Times.Never);
        _repo.Verify(r => r.DeleteAsync(It.IsAny<MenuEntry>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Upsert_DeliveryOnlyNoRecipe_ShouldCreate()
    {
        var date = new DateOnly(2026, 3, 20);

        _repo.Setup(r => r.GetByDateAsync(date, It.IsAny<CancellationToken>()))
            .ReturnsAsync((MenuEntry?)null);

        var result = await _handler.Handle(
            new UpsertMenuEntryCommand(date, null, true, "Picnic bezorging", false, false), CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.NotEqual(Guid.Empty, result.Value);
        _repo.Verify(r => r.AddAsync(
            It.Is<MenuEntry>(e => e.HasDelivery && e.DeliveryNote == "Picnic bezorging" && e.RecipeId == null),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Upsert_EatingOutOnlyNoRecipe_ShouldCreate()
    {
        var date = new DateOnly(2026, 3, 20);

        _repo.Setup(r => r.GetByDateAsync(date, It.IsAny<CancellationToken>()))
            .ReturnsAsync((MenuEntry?)null);

        var result = await _handler.Handle(
            new UpsertMenuEntryCommand(date, null, false, "", false, true), CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.NotEqual(Guid.Empty, result.Value);
        _repo.Verify(r => r.AddAsync(
            It.Is<MenuEntry>(e => e.IsEatingOut && e.RecipeId == null),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Upsert_SetsConsumedFlag()
    {
        var date = new DateOnly(2026, 3, 20);
        var recipeId = Guid.NewGuid();

        _repo.Setup(r => r.GetByDateAsync(date, It.IsAny<CancellationToken>()))
            .ReturnsAsync((MenuEntry?)null);

        var result = await _handler.Handle(
            new UpsertMenuEntryCommand(date, recipeId, false, "", true, false), CancellationToken.None);

        Assert.True(result.IsSuccess);
        _repo.Verify(r => r.AddAsync(
            It.Is<MenuEntry>(e => e.IsConsumed && e.RecipeId == recipeId),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Upsert_UpdatePreservesAllFields()
    {
        var date = new DateOnly(2026, 3, 20);
        var existing = new MenuEntry { Id = Guid.NewGuid(), Date = date };
        var recipeId = Guid.NewGuid();

        _repo.Setup(r => r.GetByDateAsync(date, It.IsAny<CancellationToken>()))
            .ReturnsAsync(existing);

        await _handler.Handle(
            new UpsertMenuEntryCommand(date, recipeId, true, "Notitie", true, true), CancellationToken.None);

        Assert.Equal(recipeId, existing.RecipeId);
        Assert.True(existing.HasDelivery);
        Assert.Equal("Notitie", existing.DeliveryNote);
        Assert.True(existing.IsConsumed);
        Assert.True(existing.IsEatingOut);
    }
}
