using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MijnKeuken.Application.Ingredients.Commands.ArchiveIngredient;
using MijnKeuken.Application.Ingredients.Commands.CreateIngredient;
using MijnKeuken.Application.Ingredients.Commands.DeleteIngredient;
using MijnKeuken.Application.Ingredients.Commands.UpdateIngredient;
using MijnKeuken.Application.Ingredients.Queries.GetIngredients;
using MijnKeuken.Application.Ingredients.Queries.ScrapeIngredient;
using MijnKeuken.Domain.Entities;

namespace MijnKeuken.Web.Controllers;

[ApiController]
[Route("api/ingredients")]
[Authorize]
public class IngredientsController(IMediator mediator) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var ingredients = await mediator.Send(new GetIngredientsQuery());
        return Ok(ingredients);
    }

    public record CreateIngredientRequest(
        string Title, string Description,
        decimal AmountAvailable, decimal AmountTotal,
        UnitType Unit, string CustomUnitDescription,
        string Barcode, string StoreUrl, bool IsOutOfStock,
        Guid? StorageLocationId, List<Guid> TagIds);

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateIngredientRequest request)
    {
        var result = await mediator.Send(new CreateIngredientCommand(
            request.Title, request.Description,
            request.AmountAvailable, request.AmountTotal,
            request.Unit, request.CustomUnitDescription,
            request.Barcode, request.StoreUrl, request.IsOutOfStock,
            request.StorageLocationId, request.TagIds));
        return result.IsSuccess ? Ok(new { id = result.Value }) : BadRequest(new { error = result.Error });
    }

    public record UpdateIngredientRequest(
        string Title, string Description,
        decimal AmountAvailable, decimal AmountTotal,
        UnitType Unit, string CustomUnitDescription,
        string Barcode, string StoreUrl, bool IsOutOfStock,
        Guid? StorageLocationId, List<Guid> TagIds);

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateIngredientRequest request)
    {
        var result = await mediator.Send(new UpdateIngredientCommand(
            id, request.Title, request.Description,
            request.AmountAvailable, request.AmountTotal,
            request.Unit, request.CustomUnitDescription,
            request.Barcode, request.StoreUrl, request.IsOutOfStock,
            request.StorageLocationId, request.TagIds));
        return result.IsSuccess ? Ok() : BadRequest(new { error = result.Error });
    }

    public record ScrapeIngredientRequest(string Url);

    [HttpPost("scrape")]
    public async Task<IActionResult> Scrape([FromBody] ScrapeIngredientRequest request)
    {
        var result = await mediator.Send(new ScrapeIngredientFromUrlQuery(request.Url));
        return result.IsSuccess ? Ok(result.Value) : BadRequest(new { error = result.Error });
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        var result = await mediator.Send(new DeleteIngredientCommand(id));
        return result.IsSuccess ? Ok() : BadRequest(new { error = result.Error });
    }

    [HttpPatch("{id:guid}/archive")]
    public async Task<IActionResult> Archive(Guid id)
    {
        var result = await mediator.Send(new ArchiveIngredientCommand(id));
        return result.IsSuccess ? Ok() : BadRequest(new { error = result.Error });
    }
}
