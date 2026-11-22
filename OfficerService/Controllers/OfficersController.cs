using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OfficerService.DTOs;
using OfficerService.Extensions;
using OfficerService.IServices;
using OfficerService.Security;

namespace OfficerService.Controllers;

[ApiController]
[Route("api/[controller]")]
[Tags("Officers")]
[Authorize]
public class OfficersController : ControllerBase
{
    private readonly IOfficerProfileService _officers;

    public OfficersController(IOfficerProfileService officers)
    {
        _officers = officers;
    }

    /// <summary>List officers, optionally filtered by region or active status.</summary>
    [HttpGet]
    public async Task<ActionResult<IEnumerable<OfficerResponseDto>>> GetAll(
        [FromQuery] string? region, [FromQuery] bool? isActive) =>
        Ok(await _officers.GetAllAsync(region, isActive));

    /// <summary>Get one officer by id.</summary>
    [HttpGet("{id:int}")]
    public async Task<ActionResult<OfficerResponseDto>> GetById(int id) =>
        Ok(await _officers.GetByIdAsync(id));

    /// <summary>Update your own profile (name, phone, region).</summary>
    [HttpPut("{id:int}")]
    public async Task<ActionResult<OfficerResponseDto>> Update(int id, UpdateOfficerDto dto) =>
        Ok(await _officers.UpdateAsync(id, User.GetOfficerId(), dto));

    /// <summary>For the Admin service: activate or deactivate an officer account. Needs the X-Api-Key header.</summary>
    [HttpPatch("{id:int}/status")]
    [AllowAnonymous]
    [ServiceKeyAuth]
    public async Task<ActionResult<OfficerResponseDto>> SetStatus(int id, SetOfficerStatusDto dto) =>
        Ok(await _officers.SetActiveAsync(id, dto.IsActive));
}
