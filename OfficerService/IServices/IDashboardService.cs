using OfficerService.DTOs;

namespace OfficerService.IServices;

public interface IDashboardService
{
    Task<DashboardResponseDto> GetOfficerDashboardAsync(int officerId);
    Task<PlatformSummaryDto> GetPlatformSummaryAsync();
}
