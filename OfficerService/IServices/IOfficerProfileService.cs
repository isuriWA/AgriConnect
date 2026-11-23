using OfficerService.DTOs;

namespace OfficerService.IServices;

public interface IOfficerProfileService
{
    Task<IEnumerable<OfficerResponseDto>> GetAllAsync(string? region, bool? isActive);
    Task<OfficerResponseDto> GetByIdAsync(int id);

    /// <summary>Officers can only edit their own profile.</summary>
    Task<OfficerResponseDto> UpdateAsync(int id, int currentOfficerId, UpdateOfficerDto dto);

    /// <summary>Used by the Admin service to activate or deactivate an account.</summary>
    Task<OfficerResponseDto> SetActiveAsync(int id, bool isActive);
}
