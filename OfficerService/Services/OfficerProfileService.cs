using Microsoft.EntityFrameworkCore;
using OfficerService.Data;
using OfficerService.DTOs;
using OfficerService.Entities;
using OfficerService.IServices;

namespace OfficerService.Services;

public class OfficerProfileService : IOfficerProfileService
{
    private readonly OfficerDbContext _db;

    public OfficerProfileService(OfficerDbContext db)
    {
        _db = db;
    }

    public async Task<IEnumerable<OfficerResponseDto>> GetAllAsync(string? region, bool? isActive)
    {
        var query = _db.Officers.AsNoTracking().AsQueryable();

        if (!string.IsNullOrWhiteSpace(region))
            query = query.Where(o => o.Region == region);

        if (isActive.HasValue)
            query = query.Where(o => o.IsActive == isActive.Value);

        var officers = await query.OrderBy(o => o.FullName).ToListAsync();
        return officers.Select(ToDto);
    }

    public async Task<OfficerResponseDto> GetByIdAsync(int id)
    {
        var officer = await _db.Officers.AsNoTracking().FirstOrDefaultAsync(o => o.Id == id)
            ?? throw new KeyNotFoundException($"Officer {id} was not found.");
        return ToDto(officer);
    }

    public async Task<OfficerResponseDto> UpdateAsync(int id, int currentOfficerId, UpdateOfficerDto dto)
    {
        if (id != currentOfficerId)
            throw new ForbiddenException("You can only edit your own profile.");

        var officer = await _db.Officers.FindAsync(id)
            ?? throw new KeyNotFoundException($"Officer {id} was not found.");

        officer.FullName = dto.FullName.Trim();
        officer.Phone = dto.Phone.Trim();
        officer.Region = dto.Region.Trim();

        await _db.SaveChangesAsync();
        return ToDto(officer);
    }

    public async Task<OfficerResponseDto> SetActiveAsync(int id, bool isActive)
    {
        var officer = await _db.Officers.FindAsync(id)
            ?? throw new KeyNotFoundException($"Officer {id} was not found.");

        officer.IsActive = isActive;
        await _db.SaveChangesAsync();
        return ToDto(officer);
    }

    private static OfficerResponseDto ToDto(Officer o) =>
        new(o.Id, o.EmployeeNumber, o.FullName, o.Email, o.Phone, o.Region, o.IsActive, o.CreatedAt, o.LastLoginAt);
}
