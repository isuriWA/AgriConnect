using System.ComponentModel.DataAnnotations;

namespace OfficerService.DTOs;

public class UpdateOfficerDto
{
    [Required, StringLength(100)]
    public string FullName { get; set; } = string.Empty;

    [Required, Phone, StringLength(20)]
    public string Phone { get; set; } = string.Empty;

    [Required, StringLength(100)]
    public string Region { get; set; } = string.Empty;
}

/// <summary>Used by the Admin service to activate or deactivate an officer account.</summary>
public class SetOfficerStatusDto
{
    public bool IsActive { get; set; }
}

public record OfficerResponseDto(
    int Id,
    string EmployeeNumber,
    string FullName,
    string Email,
    string Phone,
    string Region,
    bool IsActive,
    DateTime CreatedAt,
    DateTime? LastLoginAt);
