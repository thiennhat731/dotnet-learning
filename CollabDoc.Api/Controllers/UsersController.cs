using CollabDoc.Application.Services;
using CollabDoc.Domain.Entities;
using Microsoft.AspNetCore.Mvc;

namespace CollabDoc.Api.Controllers;

[ApiController]
[Route("api/users")]
public class UsersController : ControllerBase
{
    private readonly UserService _service;

    public UsersController(UserService service)
    {
        _service = service;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll() =>
        Ok(await _service.GetAllAsync());

    [HttpGet("{id}")]
    public async Task<IActionResult> Get(string id) =>
        Ok(await _service.GetByIdAsync(id));

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] User user)
    {
        await _service.CreateAsync(user);
        return CreatedAtAction(nameof(Get), new { id = user.Id }, user);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(string id, [FromBody] User user)
    {
        await _service.UpdateAsync(id, user);
        return Ok(new { message = "Cập nhật người dùng thành công." });
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(string id)
    {
        await _service.DeleteAsync(id);
        return Ok(new { message = "Xóa người dùng thành công." });
    }

    [HttpPatch("{id}/refresh-token")]
    public async Task<IActionResult> UpdateRefreshToken(string id, [FromBody] string refreshToken)
    {
        await _service.UpdateRefreshTokenAsync(id, refreshToken);
        return Ok(new { message = "Refresh token đã được cập nhật." });
    }
}
