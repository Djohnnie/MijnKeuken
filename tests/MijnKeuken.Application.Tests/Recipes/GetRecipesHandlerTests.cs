using MijnKeuken.Application.Interfaces;
using MijnKeuken.Application.Recipes.Queries.GetRecipes;
using MijnKeuken.Domain.Entities;
using Moq;

namespace MijnKeuken.Application.Tests.Recipes;

public class GetRecipesHandlerTests
{
    private readonly Mock<IRecipeRepository> _repo = new();
    private readonly GetRecipesHandler _handler;

    public GetRecipesHandlerTests()
    {
        _handler = new GetRecipesHandler(_repo.Object);
    }

    [Fact]
    public async Task GetRecipes_ShouldReturnAllRecipes()
    {
        var tag = new Tag { Id = Guid.NewGuid(), Name = "Italiaans", Type = TagType.Recipe, Color = "#00ff00" };
        var ingredient = new Ingredient { Id = Guid.NewGuid(), Title = "Pasta" };
        var recipeId = Guid.NewGuid();

        _repo.Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync([
                new Recipe
                {
                    Id = recipeId, Title = "Spaghetti", Description = "Lekker",
                    Plan = "## Stap 1\nKook pasta",
                    RecipeTags = [new RecipeTag { RecipeId = recipeId, TagId = tag.Id, Tag = tag }],
                    RecipeIngredients = [new RecipeIngredient
                    {
                        RecipeId = recipeId, IngredientId = ingredient.Id,
                        Ingredient = ingredient, Amount = 500, Unit = UnitType.Grams
                    }]
                }
            ]);

        var result = await _handler.Handle(new GetRecipesQuery(), CancellationToken.None);

        Assert.Single(result);
        Assert.Equal("Spaghetti", result[0].Title);
        Assert.Single(result[0].Tags);
        Assert.Equal("Italiaans", result[0].Tags[0].Name);
        Assert.Single(result[0].Ingredients);
        Assert.Equal("Pasta", result[0].Ingredients[0].IngredientTitle);
    }

    [Fact]
    public async Task GetRecipes_EmptyList_ShouldReturnEmpty()
    {
        _repo.Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync([]);

        var result = await _handler.Handle(new GetRecipesQuery(), CancellationToken.None);

        Assert.Empty(result);
    }
}
