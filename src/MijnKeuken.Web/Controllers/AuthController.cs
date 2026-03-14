using MediatR;
using Microsoft.AspNetCore.Mvc;
using MijnKeuken.Application.Users.Commands.LoginUser;
using MijnKeuken.Application.Users.Commands.RegisterUser;

namespace MijnKeuken.Web.Controllers;

[ApiController]
[Route("api/auth")]
public class AuthController(IMediator mediator) : ControllerBase
{
    public record RegisterRequest(string Username, string Password, string Email);
    public record LoginRequest(string Username, string Password);

    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] RegisterRequest request)
    {
        var result = await mediator.Send(
            new RegisterUserCommand(request.Username, request.Password, request.Email));
        return result.IsSuccess ? Ok() : BadRequest(new { error = result.Error });
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginRequest request)
    {
        var result = await mediator.Send(
            new LoginUserCommand(request.Username, request.Password));
        return result.IsSuccess
            ? Ok(new { token = result.Value })
            : Unauthorized(new { error = result.Error });
    }
}
