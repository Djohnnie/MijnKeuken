using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MijnKeuken.Application.Menu.Commands.DeleteMenuEntry;
using MijnKeuken.Application.Menu.Commands.UpsertMenuEntry;
using MijnKeuken.Application.Menu.Queries.GetMenuEntries;

namespace MijnKeuken.Web.Controllers;

[ApiController]
[Route("api/menu")]
[Authorize]
public class MenuController(IMediator mediator) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> GetByRange([FromQuery] DateOnly from, [FromQuery] DateOnly to)
    {
        var entries = await mediator.Send(new GetMenuEntriesQuery(from, to));
        return Ok(entries);
    }

    public record UpsertMenuEntryRequest(DateOnly Date, Guid? RecipeId, bool HasDelivery, string DeliveryNote, bool IsConsumed, bool IsEatingOut);

    [HttpPut]
    public async Task<IActionResult> Upsert([FromBody] UpsertMenuEntryRequest request)
    {
        var result = await mediator.Send(new UpsertMenuEntryCommand(
            request.Date, request.RecipeId, request.HasDelivery, request.DeliveryNote, request.IsConsumed, request.IsEatingOut));
        return result.IsSuccess ? Ok(new { id = result.Value }) : BadRequest(new { error = result.Error });
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        var result = await mediator.Send(new DeleteMenuEntryCommand(id));
        return result.IsSuccess ? Ok() : BadRequest(new { error = result.Error });
    }
}
