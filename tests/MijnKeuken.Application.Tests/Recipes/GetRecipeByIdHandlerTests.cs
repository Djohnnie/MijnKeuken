using MijnKeuken.Application.Interfaces;
using MijnKeuken.Application.Recipes.Queries.GetRecipeById;
using MijnKeuken.Domain.Entities;
using Moq;

namespace MijnKeuken.Application.Tests.Recipes;

public class GetRecipeByIdHandlerTests
{
    private readonly Mock<IRecipeRepository> _repo = new();
    private readonly GetRecipeByIdHandler _handler;

    public GetRecipeByIdHandlerTests()
    {
        _handler = new GetRecipeByIdHandler(_repo.Object);
    }

    [Fact]
    public async Task GetRecipeById_ExistingRecipe_ShouldReturnRecipeDto()
    {
        var id = Guid.NewGuid();
        var tag = new Tag { Id = Guid.NewGuid(), Name = "Italiaans", Type = TagType.Recipe, Color = "#ff0000" };
        var ingredient = new Ingredient { Id = Guid.NewGuid(), Title = "Pasta" };
        var recipe = new Recipe
        {
            Id = id,
            Title = "Spaghetti",
            Description = "Lekker recept",
            Plan = "## Stap 1\nKook pasta",
            Servings = 4,
            SourceUrl = "https://example.com",
            RecipeTags = [new RecipeTag { RecipeId = id, TagId = tag.Id, Tag = tag }],
            RecipeIngredients =
            [
                new RecipeIngredient
                {
                    RecipeId = id,
                    IngredientId = ingredient.Id,
                    Ingredient = ingredient,
                    Amount = 500,
                    Unit = UnitType.Grams,
                    SortOrder = 0
                }
            ]
        };

        _repo.Setup(r => r.GetByIdAsync(id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(recipe);

        var result = await _handler.Handle(new GetRecipeByIdQuery(id), CancellationToken.None);

        Assert.NotNull(result);
        Assert.Equal(id, result.Id);
        Assert.Equal("Spaghetti", result.Title);
        Assert.Equal("Lekker recept", result.Description);
        Assert.Equal(4, result.Servings);
        Assert.Single(result.Tags);
        Assert.Equal("Italiaans", result.Tags[0].Name);
        Assert.Single(result.Ingredients);
        Assert.Equal("Pasta", result.Ingredients[0].IngredientTitle);
        Assert.Equal(500, result.Ingredients[0].Amount);
    }

    [Fact]
    public async Task GetRecipeById_NonExistentRecipe_ShouldReturnNull()
    {
        _repo.Setup(r => r.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Recipe?)null);

        var result = await _handler.Handle(new GetRecipeByIdQuery(Guid.NewGuid()), CancellationToken.None);

        Assert.Null(result);
    }

    [Fact]
    public async Task GetRecipeById_ShouldOrderIngredientsBySortOrder()
    {
        var id = Guid.NewGuid();
        var recipe = new Recipe
        {
            Id = id,
            Title = "Salade",
            RecipeTags = [],
            RecipeIngredients =
            [
                new RecipeIngredient { FreeText = "Kaas", Amount = 100, Unit = UnitType.Grams, SortOrder = 2 },
                new RecipeIngredient { FreeText = "Sla", Amount = 200, Unit = UnitType.Grams, SortOrder = 0 },
                new RecipeIngredient { FreeText = "Tomaat", Amount = 50, Unit = UnitType.Grams, SortOrder = 1 }
            ]
        };

        _repo.Setup(r => r.GetByIdAsync(id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(recipe);

        var result = await _handler.Handle(new GetRecipeByIdQuery(id), CancellationToken.None);

        Assert.NotNull(result);
        Assert.Equal(3, result.Ingredients.Count);
        Assert.Equal(0, result.Ingredients[0].SortOrder);
        Assert.Equal("Sla", result.Ingredients[0].FreeText);
        Assert.Equal(1, result.Ingredients[1].SortOrder);
        Assert.Equal("Tomaat", result.Ingredients[1].FreeText);
        Assert.Equal(2, result.Ingredients[2].SortOrder);
        Assert.Equal("Kaas", result.Ingredients[2].FreeText);
    }

    [Fact]
    public async Task GetRecipeById_RecipeWithNoTagsOrIngredients_ShouldReturnEmptyLists()
    {
        var id = Guid.NewGuid();
        var recipe = new Recipe
        {
            Id = id,
            Title = "Simpel gerecht",
            RecipeTags = [],
            RecipeIngredients = []
        };

        _repo.Setup(r => r.GetByIdAsync(id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(recipe);

        var result = await _handler.Handle(new GetRecipeByIdQuery(id), CancellationToken.None);

        Assert.NotNull(result);
        Assert.Empty(result.Tags);
        Assert.Empty(result.Ingredients);
    }

    [Fact]
    public async Task GetRecipeById_ManagedIngredient_ShouldUseIngredientTitle()
    {
        var id = Guid.NewGuid();
        var ingredient = new Ingredient { Id = Guid.NewGuid(), Title = "Mozzarella" };
        var recipe = new Recipe
        {
            Id = id,
            Title = "Pizza",
            RecipeTags = [],
            RecipeIngredients =
            [
                new RecipeIngredient
                {
                    IngredientId = ingredient.Id,
                    Ingredient = ingredient,
                    FreeText = string.Empty,
                    Amount = 250,
                    Unit = UnitType.Grams,
                    SortOrder = 0
                }
            ]
        };

        _repo.Setup(r => r.GetByIdAsync(id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(recipe);

        var result = await _handler.Handle(new GetRecipeByIdQuery(id), CancellationToken.None);

        Assert.NotNull(result);
        Assert.Equal("Mozzarella", result.Ingredients[0].IngredientTitle);
        Assert.True(result.Ingredients[0].IsManaged);
    }

    [Fact]
    public async Task GetRecipeById_FreeTextIngredient_ShouldUseFreeText()
    {
        var id = Guid.NewGuid();
        var recipe = new Recipe
        {
            Id = id,
            Title = "Soep",
            RecipeTags = [],
            RecipeIngredients =
            [
                new RecipeIngredient
                {
                    IngredientId = null,
                    Ingredient = null,
                    FreeText = "Zout naar smaak",
                    Amount = 1,
                    Unit = UnitType.Units,
                    SortOrder = 0
                }
            ]
        };

        _repo.Setup(r => r.GetByIdAsync(id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(recipe);

        var result = await _handler.Handle(new GetRecipeByIdQuery(id), CancellationToken.None);

        Assert.NotNull(result);
        Assert.Equal("Zout naar smaak", result.Ingredients[0].IngredientTitle);
        Assert.False(result.Ingredients[0].IsManaged);
    }
}
