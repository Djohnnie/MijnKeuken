using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MijnKeuken.Application.Tags.Commands.CreateTag;
using MijnKeuken.Application.Tags.Commands.DeleteTag;
using MijnKeuken.Application.Tags.Commands.UpdateTag;
using MijnKeuken.Application.Tags.Queries.GetTags;
using MijnKeuken.Domain.Entities;

namespace MijnKeuken.Web.Controllers;

[ApiController]
[Route("api/tags")]
[Authorize]
public class TagsController(IMediator mediator) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var tags = await mediator.Send(new GetTagsQuery());
        return Ok(tags);
    }

    public record CreateTagRequest(string Name, TagType Type, string Color);

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateTagRequest request)
    {
        var result = await mediator.Send(new CreateTagCommand(request.Name, request.Type, request.Color));
        return result.IsSuccess ? Ok(new { id = result.Value }) : BadRequest(new { error = result.Error });
    }

    public record UpdateTagRequest(string Name, TagType Type, string Color);

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateTagRequest request)
    {
        var result = await mediator.Send(new UpdateTagCommand(id, request.Name, request.Type, request.Color));
        return result.IsSuccess ? Ok() : BadRequest(new { error = result.Error });
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        var result = await mediator.Send(new DeleteTagCommand(id));
        return result.IsSuccess ? Ok() : BadRequest(new { error = result.Error });
    }
}
