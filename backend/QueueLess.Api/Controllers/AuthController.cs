using Microsoft.AspNetCore.Mvc;
using QueueLess.Api.Services;

namespace QueueLess.Api.Controllers;

[ApiController]
[Route("api/auth")]
public class AuthController(AuthService auth) : ControllerBase
{
    [HttpPost("login")]
    public IActionResult Login(LoginRequest request)
    {
        var user = auth.Validate(request.Email, request.Password);
        if (user == null) return Unauthorized(new { message = "Invalid credentials" });
        return Ok(new { token = auth.Token(user), user = new { user.Id, user.Name, user.Email, user.Role } });
    }

    [HttpPost("register")]
    public IActionResult Register(RegisterRequest request)
    {
        try
        {
            var user = auth.Create(request.Name, request.Email, request.Password);
            return Ok(new { token = auth.Token(user), user = new { user.Id, user.Name, user.Email, user.Role } });
        }
        catch { return Conflict(new { message = "Could not create account." }); }
    }
}

public record LoginRequest(string Email, string Password);
public record RegisterRequest(string Name, string Email, string Password);
