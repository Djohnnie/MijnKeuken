using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MijnKeuken.Application.Recipes.Commands.CreateRecipe;
using MijnKeuken.Application.Recipes.Commands.DeleteRecipe;
using MijnKeuken.Application.Recipes.Commands.UpdateRecipe;
using MijnKeuken.Application.Recipes.Queries.GetRecipeById;
using MijnKeuken.Application.Recipes.Queries.GetRecipes;
using MijnKeuken.Domain.Entities;

namespace MijnKeuken.Web.Controllers;

[ApiController]
[Route("api/recipes")]
[Authorize]
public class RecipesController(IMediator mediator) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var recipes = await mediator.Send(new GetRecipesQuery());
        return Ok(recipes);
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id)
    {
        var recipe = await mediator.Send(new GetRecipeByIdQuery(id));
        return recipe is not null ? Ok(recipe) : NotFound();
    }

    public record RecipeIngredientRequest(Guid IngredientId, decimal Amount, UnitType Unit, string CustomUnitDescription);

    public record CreateRecipeRequest(
        string Title, string Description, string Plan,
        List<Guid> TagIds, List<RecipeIngredientRequest> Ingredients);

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateRecipeRequest request)
    {
        var result = await mediator.Send(new CreateRecipeCommand(
            request.Title, request.Description, request.Plan,
            request.TagIds,
            request.Ingredients.Select(i =>
                new RecipeIngredientInput(i.IngredientId, i.Amount, i.Unit, i.CustomUnitDescription)).ToList()));
        return result.IsSuccess ? Ok(new { id = result.Value }) : BadRequest(new { error = result.Error });
    }

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(Guid id, [FromBody] CreateRecipeRequest request)
    {
        var result = await mediator.Send(new UpdateRecipeCommand(
            id, request.Title, request.Description, request.Plan,
            request.TagIds,
            request.Ingredients.Select(i =>
                new RecipeIngredientInput(i.IngredientId, i.Amount, i.Unit, i.CustomUnitDescription)).ToList()));
        return result.IsSuccess ? Ok() : BadRequest(new { error = result.Error });
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        var result = await mediator.Send(new DeleteRecipeCommand(id));
        return result.IsSuccess ? Ok() : BadRequest(new { error = result.Error });
    }
}
