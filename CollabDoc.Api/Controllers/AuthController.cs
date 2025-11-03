using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using CollabDoc.Application.Dtos;
using CollabDoc.Application.Security;
using CollabDoc.Application.Services;
using Google.Apis.Auth;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

[ApiController]
[Route("api/auth")]
public class AuthController : ControllerBase
{
    private readonly UserService _userService;
    private readonly JwtTokenService _jwtService;
    public AuthController(UserService userService, JwtTokenService jwtService)
    {
        _userService = userService;
        _jwtService = jwtService;
    }
    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] UserLoginDto request)
    {
        var response = await _userService.LoginAsync(request);
        Response.Cookies.Append("refresh_token", response.RefreshToken, new CookieOptions
        {
            HttpOnly = true,         // không truy cập từ JS
            Secure = true,           // chỉ gửi qua HTTPS
            SameSite = SameSiteMode.Strict,
            Expires = DateTime.UtcNow.AddDays(7) // refresh token sống lâu hơn access_token
        });
        return Ok(new
        {
            user = new
            {
                response.User.Id,
                response.User.Email
            },
            access_token = response.AccessToken
        });
    }
    [HttpGet("refresh")]
    public async Task<IActionResult> RefreshToken()
    {
        // Lấy refresh_token từ cookie
        if (!Request.Cookies.TryGetValue("refresh_token", out var refreshToken) || string.IsNullOrWhiteSpace(refreshToken))
            return Unauthorized(new { message = "Refresh token is required" });

        // Kiểm tra refresh token hợp lệ (nếu bạn dùng JWT refresh token)
        var validated = _jwtService.ValidateToken(refreshToken, validateLifetime: true);
        if (validated == null)
            return Unauthorized(new { message = "Invalid or expired refresh token" });

        var email = validated.Claims.FirstOrDefault(c => c.Type == JwtRegisteredClaimNames.Email)?.Value;
        if (string.IsNullOrWhiteSpace(email))
            return Unauthorized(new { message = "Invalid refresh token payload" });

        // Lấy user từ DB
        var user = await _userService.GetByEmailAsync(email);
        if (user == null || user.RefreshToken != refreshToken)
            return Unauthorized(new { message = "Refresh token không hợp lệ hoặc đã bị thu hồi" });

        // Tạo access token mới + refresh token mới
        var newAccessToken = _jwtService.GenerateAccessToken(user);
        var newRefreshToken = _jwtService.GenerateRefreshTokenJwt(user.Email);

        // Cập nhật refresh token vào DB
        await _userService.UpdateRefreshTokenAsync(user.Id, newRefreshToken);

        // Set lại cookie refresh token mới
        Response.Cookies.Append("refresh_token", newRefreshToken, new CookieOptions
        {
            HttpOnly = true,
            Secure = true,
            SameSite = SameSiteMode.Strict,
            Expires = DateTime.UtcNow.AddDays(7)
        });

        // Trả về token mới
        return Ok(new
        {
            access_token = newAccessToken,
            user = new
            {
                user.Id,
                user.Email
            }
        });
    }
    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] UserRegisterDto dto)
    {
        try
        {
            var user = await _userService.RegisterAsync(dto);
            return Ok(new
            {
                message = "Đăng ký thành công",
                user = new { user.Id, user.Email, user.CreatedAt }
            });
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpPost("logout")]
    [Authorize]
    public async Task<IActionResult> Logout()
    {
        // lấy refresh token từ cookie (hoặc access token)
        if (!Request.Cookies.TryGetValue("refresh_token", out var refreshToken) || string.IsNullOrWhiteSpace(refreshToken))
            return Unauthorized(new { message = "Không có refresh token trong cookie" });

        var validated = _jwtService.ValidateToken(refreshToken, validateLifetime: false);
        if (validated == null)
            return Unauthorized(new { message = "Refresh token không hợp lệ" });

        var userId = validated.Claims.FirstOrDefault(c => c.Type == "nameid" || c.Type == JwtRegisteredClaimNames.Sub)?.Value;
        if (string.IsNullOrWhiteSpace(userId))
            return Unauthorized(new { message = "Không thể xác định người dùng" });

        await _userService.LogoutAsync(userId);

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
        // Lấy thông tin user từ JWT claims
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        Console.WriteLine($"[AuthController] Current User ID: {userId}");
        var email = User.FindFirstValue(ClaimTypes.Email);

        if (string.IsNullOrEmpty(userId) || string.IsNullOrEmpty(email))
            return Unauthorized(new { message = "Token không hợp lệ hoặc hết hạn" });

        // Lấy thông tin chi tiết từ DB
        var user = await _userService.GetByEmailAsync(email);
        if (user == null)
            return NotFound(new { message = "Không tìm thấy người dùng" });

        return Ok(new
        {
            user.Id,
            user.Email,
            user.CreatedAt
        });
    }
    [HttpPost("google")]
    public async Task<IActionResult> GoogleLogin([FromBody] TokenRequest request)
    {
        try
        {
            // 1️⃣ Xác thực token Google
            var payload = await GoogleJsonWebSignature.ValidateAsync(
                request.AccessToken,
                new GoogleJsonWebSignature.ValidationSettings()
            );

            if (payload == null || string.IsNullOrWhiteSpace(payload.Email))
                return BadRequest(new { message = "Token Google không hợp lệ" });

            var userEmail = payload.Email;
            var userName = payload.Name ?? userEmail.Split('@')[0];

            // 2️⃣ Lấy user hoặc tạo mới
            var user = await _userService.GetByEmailAsync(userEmail);
            if (user == null)
            {
                user = await _userService.RegisterAsync(new UserRegisterDto
                {
                    Email = userEmail,
                    Password = Guid.NewGuid().ToString(), // random, không dùng để login
                });
            }

            // 3️⃣ Tạo access token + refresh token hệ thống
            var accessToken = _jwtService.GenerateAccessToken(user);
            var refreshToken = _jwtService.GenerateRefreshTokenJwt(user.Email);

            // 4️⃣ Cập nhật refresh token vào DB
            await _userService.UpdateRefreshTokenAsync(user.Id, refreshToken);

            // 5️⃣ Gắn cookie refresh token
            Response.Cookies.Append("refresh_token", refreshToken, new CookieOptions
            {
                HttpOnly = true,
                Secure = true,
                SameSite = SameSiteMode.Strict,
                Expires = DateTime.UtcNow.AddDays(7)
            });

            // 6️⃣ Trả về thông tin user + access token
            return Ok(new
            {
                message = "Đăng nhập Google thành công",
                user = new
                {
                    user.Id,
                    user.Email,
                },
                access_token = accessToken
            });
        }
        catch (InvalidJwtException ex)
        {
            return BadRequest(new { message = "Mã token Google không hợp lệ", error = ex.Message });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "Lỗi máy chủ khi xác thực Google", error = ex.Message });
        }
    }

    public class TokenRequest
    {
        public string AccessToken { get; set; } = string.Empty;
    }
}