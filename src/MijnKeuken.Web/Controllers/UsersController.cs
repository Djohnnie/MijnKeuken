using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MijnKeuken.Application.Users.Commands.ApproveUser;
using MijnKeuken.Application.Users.Queries.GetUserProfile;
using MijnKeuken.Application.Interfaces;
using MijnKeuken.Application.Users.DTOs;

namespace MijnKeuken.Web.Controllers;

[ApiController]
[Route("api/users")]
[Authorize]
public class UsersController(IMediator mediator, IUserRepository userRepository) : ControllerBase
{
    [HttpGet("profile")]
    public async Task<IActionResult> GetProfile()
    {
        var userId = GetUserId();
        var result = await mediator.Send(new GetUserProfileQuery(userId));
        return result.IsSuccess ? Ok(result.Value) : NotFound(new { error = result.Error });
    }

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var users = await userRepository.GetAllUsersAsync();
        var dtos = users.Select(u => new UserListItemDto(u.Id, u.Username, u.Email, u.IsApproved, u.CreatedAt));
        return Ok(dtos);
    }

    [HttpGet("pending")]
    public async Task<IActionResult> GetPending()
    {
        var pending = await userRepository.GetPendingUsersAsync();
        var dtos = pending.Select(u => new PendingUserDto(u.Id, u.Username, u.Email, u.CreatedAt));
        return Ok(dtos);
    }

    [HttpPost("{id:guid}/approve")]
    public async Task<IActionResult> Approve(Guid id)
    {
        var userId = GetUserId();
        var result = await mediator.Send(new ApproveUserCommand(userId, id));
        return result.IsSuccess ? Ok() : BadRequest(new { error = result.Error });
    }

    private Guid GetUserId() =>
        Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value
            ?? User.FindFirst(JwtRegisteredClaimNames.Sub)?.Value
            ?? throw new UnauthorizedAccessException());
}