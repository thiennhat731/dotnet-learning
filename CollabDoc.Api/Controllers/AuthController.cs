using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using CollabDoc.Application.Dtos;
using CollabDoc.Application.Dtos.Auth;
using CollabDoc.Application.Features.Auth.Commands;
using CollabDoc.Application.Features.Auth.Queries;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

[ApiController]
[Route("api/auth")]
public class AuthController : ControllerBase
{
    private readonly IMediator _mediator;
    public AuthController(IMediator mediator)
    {
        _mediator = mediator;
    }
    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] UserLoginDto request)
    {
        var response = await _mediator.Send(new LoginCommand(request));
        Response.Cookies.Append("refresh_token", response.RefreshToken, new CookieOptions
        {
            HttpOnly = true,         // không truy cập từ JS
            Secure = true,           // chỉ gửi qua HTTPS
            SameSite = SameSiteMode.Strict,
            Expires = DateTime.UtcNow.AddDays(7) // refresh token sống lâu hơn access_token
        });
        return Ok(new
        {
            user = response.User,
            access_token = response.AccessToken
        });
    }
    [HttpGet("refresh")]
    public async Task<IActionResult> RefreshToken()
    {
        // Lấy refresh_token từ cookie
        if (!Request.Cookies.TryGetValue("refresh_token", out var refreshToken) || string.IsNullOrWhiteSpace(refreshToken))
            return Unauthorized(new { message = "Refresh token is required" });
        var command = new RefreshTokenCommand(refreshToken);
        var response = await _mediator.Send(command);
        // Set lại cookie refresh token mới
        Response.Cookies.Append("refresh_token", response.RefreshToken, new CookieOptions
        {
            HttpOnly = true,
            Secure = true,
            SameSite = SameSiteMode.Strict,
            Expires = DateTime.UtcNow.AddDays(7)
        });

        // Trả về token mới
        return Ok(new
        {
            access_token = response.AccessToken,
            user = response.User
        });
    }
    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] UserRegisterDto dto)
    {
        try
        {
            var response = await _mediator.Send(new RegisterCommand(dto));
            return Ok(new
            {
                message = "Đăng ký thành công",
                user = new
                {
                    Id = response.Id,
                    Email = response.Email,
                    CreatedAt = response.CreatedAt,
                    UpdatedAt = response.UpdatedAt
                }
            });
        }
        catch (Exception ex)
        {
            return BadRequest(new
            {
                message = ex.Message
            });
        }
    }

    [HttpPost("logout")]
    [Authorize]
    public async Task<IActionResult> Logout()
    {
        // lấy refresh token từ cookie (hoặc access token)
        if (!Request.Cookies.TryGetValue("refresh_token", out var refreshToken) || string.IsNullOrWhiteSpace(refreshToken))
            return Unauthorized(new { message = "Không có refresh token trong cookie" });
        var command = new LogoutCommand(refreshToken);
        await _mediator.Send(command);

        // Xóa cookie refresh token
        Response.Cookies.Delete("refresh_token", new CookieOptions
        {
            HttpOnly = true,
            Secure = true,
            SameSite = SameSiteMode.Strict
        });

        return Ok(new { message = "Đăng xuất thành công" });
    }
    [HttpGet("account")]
    [Authorize]
    public async Task<IActionResult> GetCurrentAccount()
    {

        var email = User.FindFirstValue(ClaimTypes.Email);

        if (string.IsNullOrEmpty(email))
            return Unauthorized(new { message = "Token không hợp lệ hoặc hết hạn" });
        var query = new GetCurrentUserQuery(email);
        var response = await _mediator.Send(query);

        return Ok(new
        {
            response.Id,
            response.Email,
            response.CreatedAt,
            response.UpdatedAt
        });

    }
    [HttpPost("google")]
    public async Task<IActionResult> GoogleLogin([FromBody] LoginWithGoogleCommand command)
    {
        var response = await _mediator.Send(command);
        Response.Cookies.Append("refresh_token", response.RefreshToken, new CookieOptions
        {
            HttpOnly = true,         // không truy cập từ JS
            Secure = true,           // chỉ gửi qua HTTPS
            SameSite = SameSiteMode.Strict,
            Expires = DateTime.UtcNow.AddDays(7) // refresh token sống lâu hơn access_token
        });

        return Ok(new
        {
            message = "Đăng nhập Google thành công",
            user = new
            {
                response.User.Id,
                response.User.Email,
                response.User.CreatedAt,
                response.User.UpdatedAt
            },
            access_token = response.AccessToken
        });
    }
}

public class TokenRequest
{
    public string AccessToken { get; set; } = string.Empty;
}