using MijnKeuken.Application.Ingredients.Queries.GetIngredients;
using MijnKeuken.Application.Interfaces;
using MijnKeuken.Domain.Entities;
using Moq;

namespace MijnKeuken.Application.Tests.Ingredients;

public class GetIngredientsHandlerTests
{
    private readonly Mock<IIngredientRepository> _repo = new();
    private readonly GetIngredientsHandler _handler;

    public GetIngredientsHandlerTests()
    {
        _handler = new GetIngredientsHandler(_repo.Object);
    }

    [Fact]
    public async Task GetIngredients_ShouldReturnAllIngredients()
    {
        var tag = new Tag { Id = Guid.NewGuid(), Name = "Groente", Type = TagType.Ingredient, Color = "#00ff00" };
        var storage = new StorageLocation { Id = Guid.NewGuid(), Name = "Koelkast" };
        var ingredientId = Guid.NewGuid();

        _repo.Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync([
                new Ingredient
                {
                    Id = ingredientId, Title = "Tomaat", Description = "Rode tomaat",
                    AmountAvailable = 5, AmountTotal = 10, Unit = UnitType.Units,
                    StorageLocationId = storage.Id, StorageLocation = storage,
                    IngredientTags = [new IngredientTag { IngredientId = ingredientId, TagId = tag.Id, Tag = tag }]
                }
            ]);

        var result = await _handler.Handle(new GetIngredientsQuery(), CancellationToken.None);

        Assert.Single(result);
        Assert.Equal("Tomaat", result[0].Title);
        Assert.Equal("Koelkast", result[0].StorageLocationName);
        Assert.Single(result[0].Tags);
        Assert.Equal("Groente", result[0].Tags[0].Name);
    }

    [Fact]
    public async Task GetIngredients_EmptyList_ShouldReturnEmpty()
    {
        _repo.Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync([]);

        var result = await _handler.Handle(new GetIngredientsQuery(), CancellationToken.None);

        Assert.Empty(result);
    }

    [Fact]
    public async Task GetIngredients_WithoutStorageLocation_ShouldReturnNullName()
    {
        var ingredientId = Guid.NewGuid();
        _repo.Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync([
                new Ingredient
                {
                    Id = ingredientId, Title = "Zout", IngredientTags = []
                }
            ]);

        var result = await _handler.Handle(new GetIngredientsQuery(), CancellationToken.None);

        Assert.Single(result);
        Assert.Null(result[0].StorageLocationName);
    }
}
