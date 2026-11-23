using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OfficerService.DTOs;
using OfficerService.Extensions;
using OfficerService.IServices;
using OfficerService.Security;

namespace OfficerService.Controllers;

[ApiController]
[Route("api/[controller]")]
[Tags("Dashboard")]
[Authorize]
public class DashboardController : ControllerBase
{
    private readonly IDashboardService _dashboard;

    public DashboardController(IDashboardService dashboard)
    {
        _dashboard = dashboard;
    }

    /// <summary>The logged-in officer's dashboard: verification stats, compliance stats, recommendations and items needing attention.</summary>
    [HttpGet]
    public async Task<ActionResult<DashboardResponseDto>> Get() =>
        Ok(await _dashboard.GetOfficerDashboardAsync(User.GetOfficerId()));

    /// <summary>Admin service: platform-wide verification and compliance numbers. Needs the X-Api-Key header.</summary>
    [HttpGet("platform-summary")]
    [AllowAnonymous]
    [ServiceKeyAuth]
    public async Task<ActionResult<PlatformSummaryDto>> PlatformSummary() =>
        Ok(await _dashboard.GetPlatformSummaryAsync());
}
