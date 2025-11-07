using CollabDoc.Application.Dtos;
using CollabDoc.Application.Features.Users.Commands;
using CollabDoc.Application.Features.Users.Queries;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace CollabDoc.Api.Controllers;

[ApiController]
[Route("api/users")]
public class UsersController : ControllerBase
{
    private readonly IMediator _mediator;

    public UsersController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll() =>
        Ok(await _mediator.Send(new GetAllUsersQuery()));

    [HttpGet("{id}")]
    public async Task<IActionResult> Get(string id) =>
        Ok(await _mediator.Send(new GetUserByIdQuery(id)));

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateUserRequest request)
    {
        var result = await _mediator.Send(new CreateUserCommand(request));
        return CreatedAtAction(nameof(Get), new { id = result.Id }, result);
    }


    [HttpPut("{id}")]
    public async Task<IActionResult> Update([FromBody] UpdateUserRequest user)
    {
        await _mediator.Send(new UpdateUserCommand(user));
        return Ok(new { message = "Cập nhật người dùng thành công." });
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(string id)
    {
        await _mediator.Send(new DeleteUserCommand(id));
        return Ok(new { message = "Xóa người dùng thành công." });
    }
}
