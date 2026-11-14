using EnglishPractice.Application.DTOs;
using EnglishPractice.Application.DTOs.Auth;
using EnglishPractice.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace EnglishPractice.API.Controllers;

[ApiController]
[Route("api/auth")]
public class AuthController : ControllerBase
{
    private readonly IAuthService _auth;
    public AuthController(IAuthService auth) => _auth = auth;

    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] RegisterRequest request, CancellationToken ct)
    {
        var correlationId = HttpContext.Items["CorrelationId"]?.ToString();
        var result = await _auth.RegisterAsync(request, ct);
        return Ok(ApiResponse<AuthResponse>.Ok(result, correlationId));
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginRequest request, CancellationToken ct)
    {
        var correlationId = HttpContext.Items["CorrelationId"]?.ToString();
        var result = await _auth.LoginAsync(request, ct);
        return Ok(ApiResponse<AuthResponse>.Ok(result, correlationId));
    }

    [HttpPost("refresh")]
    public async Task<IActionResult> Refresh([FromBody] RefreshRequest request, CancellationToken ct)
    {
        var correlationId = HttpContext.Items["CorrelationId"]?.ToString();
        var result = await _auth.RefreshAsync(request, ct);
        return Ok(ApiResponse<AuthResponse>.Ok(result, correlationId));
    }
}
