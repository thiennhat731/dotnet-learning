using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using day11.Application.Features.Auth.Commands;
// using day11.Application.Features.Auth.Queries;

namespace day11.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IMediator _mediator;

    public AuthController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginCommand command)
    {
        var result = await _mediator.Send(command);
        return Ok(result);
    }

    // [HttpPost("register")]
    // public async Task<IActionResult> Register([FromBody] RegisterCommand command)
    // {
    //     var result = await _mediator.Send(command);
    //     return Ok(result);
    // }

    // [HttpPost("refresh")]
    // public async Task<IActionResult> Refresh([FromBody] RefreshTokenCommand command)
    // {
    //     var result = await _mediator.Send(command);
    //     return Ok(result);
    // }

    // [HttpPost("google")]
    // public async Task<IActionResult> GoogleLogin([FromBody] GoogleLoginCommand command)
    // {
    //     var result = await _mediator.Send(command);
    //     return Ok(result);
    // }

    // [Authorize]
    // [HttpGet("account")]
    // public async Task<IActionResult> GetCurrentUser()
    // {
    //     var query = new GetCurrentUserQuery(User);
    //     var result = await _mediator.Send(query);
    //     return Ok(result);
    // }

    // [Authorize]
    // [HttpPost("logout")]
    // public async Task<IActionResult> Logout()
    // {
    //     // Có thể tạo LogoutCommand nếu cần
    //     return Ok(new { message = "Đăng xuất thành công" });
    // }
}
