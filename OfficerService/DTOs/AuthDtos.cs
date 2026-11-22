using System.ComponentModel.DataAnnotations;

namespace OfficerService.DTOs;

public class RegisterOfficerDto
{
    [Required, StringLength(20)]
    public string EmployeeNumber { get; set; } = string.Empty;

    [Required, StringLength(100)]
    public string FullName { get; set; } = string.Empty;

    [Required, EmailAddress, StringLength(150)]
    public string Email { get; set; } = string.Empty;

    [Required, Phone, StringLength(20)]
    public string Phone { get; set; } = string.Empty;

    /// <summary>District / area the officer covers, e.g. "Gampaha". Requests from this region are assigned to the officer.</summary>
    [Required, StringLength(100)]
    public string Region { get; set; } = string.Empty;

    [Required, StringLength(100, MinimumLength = 8)]
    [RegularExpression(@"^(?=.*[A-Za-z])(?=.*\d).+$", ErrorMessage = "Password must contain at least one letter and one number.")]
    public string Password { get; set; } = string.Empty;

    [Required, Compare(nameof(Password), ErrorMessage = "Passwords do not match.")]
    public string ConfirmPassword { get; set; } = string.Empty;
}

public class LoginDto
{
    [Required, EmailAddress]
    public string Email { get; set; } = string.Empty;

    [Required]
    public string Password { get; set; } = string.Empty;
}

public class ChangePasswordDto
{
    [Required]
    public string CurrentPassword { get; set; } = string.Empty;

    [Required, StringLength(100, MinimumLength = 8)]
    [RegularExpression(@"^(?=.*[A-Za-z])(?=.*\d).+$", ErrorMessage = "Password must contain at least one letter and one number.")]
    public string NewPassword { get; set; } = string.Empty;

    [Required, Compare(nameof(NewPassword), ErrorMessage = "Passwords do not match.")]
    public string ConfirmNewPassword { get; set; } = string.Empty;
}

/// <summary>Returned by register and login. Send the token as "Authorization: Bearer {token}".</summary>
public record AuthResponseDto(string Token, DateTime ExpiresAt, OfficerResponseDto Officer);
