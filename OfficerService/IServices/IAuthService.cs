using OfficerService.DTOs;

namespace OfficerService.IServices;

public interface IAuthService
{
    Task<AuthResponseDto> RegisterAsync(RegisterOfficerDto dto);
    Task<AuthResponseDto> LoginAsync(LoginDto dto);
    Task ChangePasswordAsync(int officerId, ChangePasswordDto dto);
}
