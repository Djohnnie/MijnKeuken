using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MijnKeuken.Application.StorageLocations.Commands.CreateStorageLocation;
using MijnKeuken.Application.StorageLocations.Commands.DeleteStorageLocation;
using MijnKeuken.Application.StorageLocations.Commands.UpdateStorageLocation;
using MijnKeuken.Application.StorageLocations.Queries.GetStorageLocations;

namespace MijnKeuken.Web.Controllers;

[ApiController]
[Route("api/storage-locations")]
[Authorize]
public class StorageLocationsController(IMediator mediator) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var locations = await mediator.Send(new GetStorageLocationsQuery());
        return Ok(locations);
    }

    public record CreateStorageLocationRequest(string Name, string Description);

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateStorageLocationRequest request)
    {
        var result = await mediator.Send(new CreateStorageLocationCommand(request.Name, request.Description));
        return result.IsSuccess ? Ok(new { id = result.Value }) : BadRequest(new { error = result.Error });
    }

    public record UpdateStorageLocationRequest(string Name, string Description);

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateStorageLocationRequest request)
    {
        var result = await mediator.Send(new UpdateStorageLocationCommand(id, request.Name, request.Description));
        return result.IsSuccess ? Ok() : BadRequest(new { error = result.Error });
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        var result = await mediator.Send(new DeleteStorageLocationCommand(id));
        return result.IsSuccess ? Ok() : BadRequest(new { error = result.Error });
    }
}
