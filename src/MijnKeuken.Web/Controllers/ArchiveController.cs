using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MijnKeuken.Application.Archive.Queries.GetArchivedItems;
using MijnKeuken.Application.Ingredients.Commands.UnarchiveIngredient;
using MijnKeuken.Application.Recipes.Commands.UnarchiveRecipe;

namespace MijnKeuken.Web.Controllers;

[ApiController]
[Route("api/archive")]
[Authorize]
public class ArchiveController(IMediator mediator) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var items = await mediator.Send(new GetArchivedItemsQuery());
        return Ok(items);
    }

    [HttpPatch("recipes/{id:guid}/unarchive")]
    public async Task<IActionResult> UnarchiveRecipe(Guid id)
    {
        var result = await mediator.Send(new UnarchiveRecipeCommand(id));
        return result.IsSuccess ? Ok() : BadRequest(new { error = result.Error });
    }

    [HttpPatch("ingredients/{id:guid}/unarchive")]
    public async Task<IActionResult> UnarchiveIngredient(Guid id)
    {
        var result = await mediator.Send(new UnarchiveIngredientCommand(id));
        return result.IsSuccess ? Ok() : BadRequest(new { error = result.Error });
    }
}
