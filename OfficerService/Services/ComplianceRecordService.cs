using Microsoft.EntityFrameworkCore;
using OfficerService.Data;
using OfficerService.DTOs;
using OfficerService.Entities;
using OfficerService.IServices;

namespace OfficerService.Services;

/// <summary>Compliance monitoring: inspection records for farmers and suppliers.</summary>
public class ComplianceRecordService : IComplianceRecordService
{
    private readonly OfficerDbContext _db;

    public ComplianceRecordService(OfficerDbContext db)
    {
        _db = db;
    }

    public async Task<IEnumerable<ComplianceRecordResponseDto>> GetAllAsync(
        int? officerId, ComplianceSubjectType? subjectType, int? subjectId,
        ComplianceStatus? status, bool? reviewDue)
    {
        var query = _db.ComplianceRecords.AsNoTracking().Include(c => c.Officer).AsQueryable();

        if (officerId.HasValue) query = query.Where(c => c.OfficerId == officerId.Value);
        if (subjectType.HasValue) query = query.Where(c => c.SubjectType == subjectType.Value);
        if (subjectId.HasValue) query = query.Where(c => c.SubjectId == subjectId.Value);
        if (status.HasValue) query = query.Where(c => c.Status == status.Value);

        if (reviewDue == true)
        {
            var now = DateTime.UtcNow;
            query = query.Where(c => c.NextReviewDate != null && c.NextReviewDate <= now);
        }

        var items = await query.OrderByDescending(c => c.InspectedAt).ToListAsync();
        return items.Select(ToDto);
    }

    public async Task<ComplianceRecordResponseDto> GetByIdAsync(int id)
    {
        var item = await _db.ComplianceRecords.AsNoTracking().Include(c => c.Officer)
                .FirstOrDefaultAsync(c => c.Id == id)
            ?? throw new KeyNotFoundException($"Compliance record {id} was not found.");
        return ToDto(item);
    }

    public async Task<ComplianceRecordResponseDto> CreateAsync(int officerId, CreateComplianceRecordDto dto)
    {
        var officer = await _db.Officers.FindAsync(officerId)
            ?? throw new KeyNotFoundException($"Officer {officerId} was not found.");

        if (!officer.IsActive)
            throw new InvalidOperationException("An inactive officer cannot record inspections.");

        var inspectedAt = dto.InspectedAt ?? DateTime.UtcNow;
        Validate(dto.Status, dto.CorrectiveAction, dto.NextReviewDate, inspectedAt);

        var record = new ComplianceRecord
        {
            OfficerId = officer.Id,
            Officer = officer,
            SubjectType = dto.SubjectType,
            SubjectId = dto.SubjectId,
            SubjectName = dto.SubjectName.Trim(),
            Standard = dto.Standard.Trim(),
            Status = dto.Status,
            Findings = dto.Findings.Trim(),
            CorrectiveAction = dto.CorrectiveAction?.Trim(),
            InspectedAt = inspectedAt,
            NextReviewDate = dto.NextReviewDate
        };

        _db.ComplianceRecords.Add(record);
        await _db.SaveChangesAsync();
        return ToDto(record);
    }

    public async Task<ComplianceRecordResponseDto> UpdateAsync(int id, int officerId, UpdateComplianceRecordDto dto)
    {
        var record = await _db.ComplianceRecords.Include(c => c.Officer).FirstOrDefaultAsync(c => c.Id == id)
            ?? throw new KeyNotFoundException($"Compliance record {id} was not found.");

        if (record.OfficerId != officerId)
            throw new ForbiddenException("You can only update inspection records you created.");

        var inspectedAt = DateTime.UtcNow;
        Validate(dto.Status, dto.CorrectiveAction, dto.NextReviewDate, inspectedAt);

        record.Status = dto.Status;
        record.Findings = dto.Findings.Trim();
        record.CorrectiveAction = dto.CorrectiveAction?.Trim();
        record.NextReviewDate = dto.NextReviewDate;
        record.InspectedAt = inspectedAt;

        await _db.SaveChangesAsync();
        return ToDto(record);
    }

    private static void Validate(
        ComplianceStatus status, string? correctiveAction, DateTime? nextReviewDate, DateTime inspectedAt)
    {
        if (status != ComplianceStatus.Compliant && string.IsNullOrWhiteSpace(correctiveAction))
            throw new InvalidOperationException(
                "A corrective action is required when the status is UnderWatch or NonCompliant.");

        if (nextReviewDate.HasValue && nextReviewDate.Value <= inspectedAt)
            throw new InvalidOperationException("The next review date must be after the inspection date.");
    }

    private static ComplianceRecordResponseDto ToDto(ComplianceRecord c) =>
        new(c.Id, c.OfficerId, c.Officer?.FullName ?? string.Empty, c.SubjectType, c.SubjectId,
            c.SubjectName, c.Standard, c.Status, c.Findings, c.CorrectiveAction, c.InspectedAt,
            c.NextReviewDate, c.NextReviewDate.HasValue && c.NextReviewDate.Value <= DateTime.UtcNow);
}
