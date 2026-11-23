using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OfficerService.DTOs;
using OfficerService.Extensions;
using OfficerService.IServices;

namespace OfficerService.Controllers;

[ApiController]
[Route("api/[controller]")]
[Tags("Authentication")]
public class AuthController : ControllerBase
{
    private readonly IAuthService _auth;
    private readonly IOfficerProfileService _officers;

    public AuthController(IAuthService auth, IOfficerProfileService officers)
    {
        _auth = auth;
        _officers = officers;
    }

    /// <summary>Register a new Agriculture Officer. Returns a token, so the officer is logged in straight away.</summary>
    [HttpPost("register")]
    [AllowAnonymous]
    public async Task<ActionResult<AuthResponseDto>> Register(RegisterOfficerDto dto)
    {
        var result = await _auth.RegisterAsync(dto);
        return CreatedAtAction(nameof(Me), result);
    }

    /// <summary>Log in with email and password. Send the returned token as "Authorization: Bearer {token}".</summary>
    [HttpPost("login")]
    [AllowAnonymous]
    public async Task<ActionResult<AuthResponseDto>> Login(LoginDto dto) =>
        Ok(await _auth.LoginAsync(dto));

    /// <summary>The logged-in officer's own profile.</summary>
    [HttpGet("me")]
    [Authorize]
    public async Task<ActionResult<OfficerResponseDto>> Me() =>
        Ok(await _officers.GetByIdAsync(User.GetOfficerId()));

    /// <summary>Change the logged-in officer's password.</summary>
    [HttpPost("change-password")]
    [Authorize]
    public async Task<IActionResult> ChangePassword(ChangePasswordDto dto)
    {
        await _auth.ChangePasswordAsync(User.GetOfficerId(), dto);
        return NoContent();
    }
}
