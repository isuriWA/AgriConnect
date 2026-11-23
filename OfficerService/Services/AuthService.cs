using System.Security.Claims;
using System.Text;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.IdentityModel.Tokens;
using OfficerService.Data;
using OfficerService.DTOs;
using OfficerService.Entities;
using OfficerService.IServices;
using OfficerService.Settings;

namespace OfficerService.Services;

public class AuthService : IAuthService
{
    private readonly OfficerDbContext _db;
    private readonly JwtOptions _jwt;

    public AuthService(OfficerDbContext db, IOptions<JwtOptions> jwt)
    {
        _db = db;
        _jwt = jwt.Value;
    }

    public async Task<AuthResponseDto> RegisterAsync(RegisterOfficerDto dto)
    {
        var email = dto.Email.Trim().ToLowerInvariant();
        var employeeNumber = dto.EmployeeNumber.Trim().ToUpperInvariant();

        if (await _db.Officers.AnyAsync(o => o.Email == email))
            throw new ConflictException("An officer with this email already exists.");

        if (await _db.Officers.AnyAsync(o => o.EmployeeNumber == employeeNumber))
            throw new ConflictException("An officer with this employee number already exists.");

        var officer = new Officer
        {
            EmployeeNumber = employeeNumber,
            FullName = dto.FullName.Trim(),
            Email = email,
            Phone = dto.Phone.Trim(),
            Region = dto.Region.Trim(),
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.Password),
            LastLoginAt = DateTime.UtcNow
        };

        _db.Officers.Add(officer);
        await _db.SaveChangesAsync();

        return BuildResponse(officer);
    }

    public async Task<AuthResponseDto> LoginAsync(LoginDto dto)
    {
        var email = dto.Email.Trim().ToLowerInvariant();
        var officer = await _db.Officers.FirstOrDefaultAsync(o => o.Email == email);

        // Same message for "no such email" and "wrong password" so accounts can't be probed.
        const string invalid = "Invalid email or password.";
        if (officer is null || !BCrypt.Net.BCrypt.Verify(dto.Password, officer.PasswordHash))
            throw new UnauthorizedAccessException(invalid);

        if (!officer.IsActive)
            throw new ForbiddenException("This account has been deactivated. Contact the administrator.");

        officer.LastLoginAt = DateTime.UtcNow;
        await _db.SaveChangesAsync();

        return BuildResponse(officer);
    }

    public async Task ChangePasswordAsync(int officerId, ChangePasswordDto dto)
    {
        var officer = await _db.Officers.FindAsync(officerId)
            ?? throw new KeyNotFoundException($"Officer {officerId} was not found.");

        if (!BCrypt.Net.BCrypt.Verify(dto.CurrentPassword, officer.PasswordHash))
            throw new InvalidOperationException("The current password is incorrect.");

        officer.PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.NewPassword);
        await _db.SaveChangesAsync();
    }

    private AuthResponseDto BuildResponse(Officer officer)
    {
        var expires = DateTime.UtcNow.AddMinutes(_jwt.ExpiryMinutes);

        var descriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(new[]
            {
                new Claim("sub", officer.Id.ToString()),
                new Claim("email", officer.Email),
                new Claim("name", officer.FullName),
                new Claim("role", "Officer")
            }),
            Expires = expires,
            Issuer = _jwt.Issuer,
            Audience = _jwt.Audience,
            SigningCredentials = new SigningCredentials(
                new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwt.Key)),
                SecurityAlgorithms.HmacSha256)
        };

        var token = new JsonWebTokenHandler().CreateToken(descriptor);

        var profile = new OfficerResponseDto(
            officer.Id, officer.EmployeeNumber, officer.FullName, officer.Email, officer.Phone,
            officer.Region, officer.IsActive, officer.CreatedAt, officer.LastLoginAt);

        return new AuthResponseDto(token, expires, profile);
    }
}