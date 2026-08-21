using API.Interfaces;
using API.Models.Requests;
using API.Models.Responses;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers;

public sealed class AuthController(IAuthService authService) : ApiController
{
    [Authorize]
    [HttpGet("@me")]
    [ProducesResponseType<UserDto>(StatusCodes.Status200OK)]
    public IActionResult GetCurrentUser()
    {
        var result = authService.GetCurrentUser();
        return result.Match(Ok, Problem);
    }

    [HttpPost("register")]
    [ProducesResponseType<UserDto>(StatusCodes.Status200OK)]
    public async Task<IActionResult> Register([FromBody] RegisterRequest request, CancellationToken ct)
    {
        var result = await authService.RegisterAsync(request, ct);
        return result.Match(Ok, Problem);
    }

    [HttpPost("login")]
    [ProducesResponseType<UserDto>(StatusCodes.Status200OK)]
    public async Task<IActionResult> Login([FromBody] LoginRequest request, CancellationToken ct)
    {
        var result = await authService.LoginAsync(request, ct);
        return result.Match(Ok, Problem);
    }

    [Authorize]
    [HttpPost("logout")]
    public async Task<IActionResult> LogOut(CancellationToken ct)
    {
        await authService.LogoutAsync(ct);
        return Ok();
    }
}

