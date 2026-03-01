using App.BLL.Services.Interfaces;
using App.DTO.Auth;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace WebApp.Controllers.Api;

/// <summary>
/// Authentication endpoints: login, register, refresh, logout.
/// Login and register are anonymous; all others require a valid JWT.
/// </summary>
[Route("api/[controller]")]
public class AccountController : ApiControllerBase
{
    private readonly IAuthService _authService;

    public AccountController(IAuthService authService)
    {
        _authService = authService;
    }

    // -----------------------------------------------------------------------
    // Anonymous endpointss
    // -----------------------------------------------------------------------

    /// <summary>
    /// Authenticate user and return JWT access token + refresh token.
    /// </summary>
    [AllowAnonymous]
    [HttpPost("login")]
    public async Task<ActionResult<LoginResponseDto>> Login([FromBody] LoginRequestDto dto)
    {
        var result = await _authService.LoginAsync(dto);
        if (result == null)
            return Unauthorized(new { message = "Invalid username or password." });

        return Ok(result);
    }

    /// <summary>
    /// Register a new user and return JWT access token + refresh token.
    /// </summary>
    [AllowAnonymous]
    [HttpPost("register")]
    public async Task<ActionResult<LoginResponseDto>> Register([FromBody] RegisterRequestDto dto)
    {
        var result = await _authService.RegisterAsync(dto);
        if (result == null)
            return BadRequest(new { message = "Registration failed. Username or email may already be in use." });

        return Ok(result);
    }

    /// <summary>
    /// Exchange a valid refresh token for a new access token and rotated refresh token.
    /// </summary>
    [AllowAnonymous]
    [HttpPost("refresh")]
    public async Task<ActionResult<LoginResponseDto>> Refresh([FromBody] RefreshTokenRequestDto dto)
    {
        var result = await _authService.RefreshTokenAsync(dto.RefreshToken);
        if (result == null)
            return Unauthorized(new { message = "Invalid or expired refresh token." });

        return Ok(result);
    }

    // -----------------------------------------------------------------------
    // Authenticated endpoint
    // -----------------------------------------------------------------------

    /// <summary>
    /// Revoke the supplied refresh token (logout).
    /// Requires a valid JWT in the Authorization header.
    /// </summary>
    [HttpPost("logout")]
    public async Task<IActionResult> Logout([FromBody] RefreshTokenRequestDto dto)
    {
        await _authService.LogoutAsync(dto.RefreshToken);
        // Always return 204 — don't leak whether the token existed
        return NoContent();
    }
}
