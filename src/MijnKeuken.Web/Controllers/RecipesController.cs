using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MijnKeuken.Application.Recipes.Commands.ArchiveRecipe;
using MijnKeuken.Application.Recipes.Commands.CreateRecipe;
using MijnKeuken.Application.Recipes.Commands.DeleteRecipe;
using MijnKeuken.Application.Recipes.Commands.UpdateRecipe;
using MijnKeuken.Application.Recipes.Queries.GetRecipeById;
using MijnKeuken.Application.Recipes.Queries.GetRecipes;
using MijnKeuken.Application.Recipes.Queries.ScrapeRecipe;
using MijnKeuken.Application.Recipes.Queries.ScrapeRecipeFromImage;
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

    public record RecipeIngredientRequest(Guid? IngredientId, string FreeText, decimal Amount, UnitType Unit, string CustomUnitDescription, int SortOrder);

    public record CreateRecipeRequest(
        string Title, string Description, string Plan, int Servings, string SourceUrl,
        List<Guid> TagIds, List<RecipeIngredientRequest> Ingredients);

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateRecipeRequest request)
    {
        var result = await mediator.Send(new CreateRecipeCommand(
            request.Title, request.Description, request.Plan, request.Servings, request.SourceUrl,
            request.TagIds,
            request.Ingredients.Select(i =>
                new RecipeIngredientInput(i.IngredientId, i.FreeText, i.Amount, i.Unit, i.CustomUnitDescription, i.SortOrder)).ToList()));
        return result.IsSuccess ? Ok(new { id = result.Value }) : BadRequest(new { error = result.Error });
    }

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(Guid id, [FromBody] CreateRecipeRequest request)
    {
        var result = await mediator.Send(new UpdateRecipeCommand(
            id, request.Title, request.Description, request.Plan, request.Servings, request.SourceUrl,
            request.TagIds,
            request.Ingredients.Select(i =>
                new RecipeIngredientInput(i.IngredientId, i.FreeText, i.Amount, i.Unit, i.CustomUnitDescription, i.SortOrder)).ToList()));
        return result.IsSuccess ? Ok() : BadRequest(new { error = result.Error });
    }

    public record ScrapeRecipeRequest(string Url);

    [HttpPost("scrape")]
    public async Task<IActionResult> Scrape([FromBody] ScrapeRecipeRequest request)
    {
        var result = await mediator.Send(new ScrapeRecipeFromUrlQuery(request.Url));
        return result.IsSuccess ? Ok(result.Value) : BadRequest(new { error = result.Error });
    }

    [HttpPost("scrape-image")]
    public async Task<IActionResult> ScrapeImage(IFormFile image)
    {
        if (image is null || image.Length == 0)
            return BadRequest(new { error = "Geen afbeelding ontvangen." });

        using var ms = new MemoryStream();
        await image.CopyToAsync(ms);

        var result = await mediator.Send(
            new ScrapeRecipeFromImageQuery(ms.ToArray(), image.ContentType));
        return result.IsSuccess ? Ok(result.Value) : BadRequest(new { error = result.Error });
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        var result = await mediator.Send(new DeleteRecipeCommand(id));
        return result.IsSuccess ? Ok() : BadRequest(new { error = result.Error });
    }

    [HttpPatch("{id:guid}/archive")]
    public async Task<IActionResult> Archive(Guid id)
    {
        var result = await mediator.Send(new ArchiveRecipeCommand(id));
        return result.IsSuccess ? Ok() : BadRequest(new { error = result.Error });
    }
}
