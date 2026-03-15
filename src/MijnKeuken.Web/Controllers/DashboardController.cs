using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MijnKeuken.Application.Dashboard.Queries.GetDashboardStats;

namespace MijnKeuken.Web.Controllers;

[ApiController]
[Route("api/dashboard")]
[Authorize]
public class DashboardController(IMediator mediator) : ControllerBase
{
    [HttpGet("stats")]
    public async Task<IActionResult> GetStats([FromQuery] int top = 5)
    {
        var stats = await mediator.Send(new GetDashboardStatsQuery(top));
        return Ok(stats);
    }
}
