using Microsoft.EntityFrameworkCore;
using OfficerService.Data;
using OfficerService.DTOs;
using OfficerService.Entities;
using OfficerService.IServices;

namespace OfficerService.Services;

public class DashboardService : IDashboardService
{
    private readonly OfficerDbContext _db;

    public DashboardService(OfficerDbContext db)
    {
        _db = db;
    }

    public async Task<DashboardResponseDto> GetOfficerDashboardAsync(int officerId)
    {
        var officer = await _db.Officers.AsNoTracking().FirstOrDefaultAsync(o => o.Id == officerId)
            ?? throw new KeyNotFoundException($"Officer {officerId} was not found.");

        var now = DateTime.UtcNow;

        // ----- Verifications assigned to this officer -----
        var mine = _db.VerificationRequests.AsNoTracking().Where(v => v.AssignedOfficerId == officerId);

        var statusCounts = await mine
            .GroupBy(v => v.Status)
            .Select(g => new { Status = g.Key, Count = g.Count() })
            .ToListAsync();

        int CountOf(VerificationStatus s) => statusCounts.FirstOrDefault(c => c.Status == s)?.Count ?? 0;

        var pending = CountOf(VerificationStatus.Pending);
        var underReview = CountOf(VerificationStatus.UnderReview);
        var approved = CountOf(VerificationStatus.Approved);
        var rejected = CountOf(VerificationStatus.Rejected);

        var overdue = await mine.CountAsync(v =>
            (v.Status == VerificationStatus.Pending || v.Status == VerificationStatus.UnderReview)
            && v.DueDate < now);

        var unassignedInRegion = await _db.VerificationRequests.CountAsync(v =>
            v.AssignedOfficerId == null && v.Region == officer.Region);

        var decidedTimes = await mine
            .Where(v => v.DecidedAt != null)
            .Select(v => new { v.SubmittedAt, v.DecidedAt })
            .ToListAsync();

        double? averageHours = decidedTimes.Count == 0
            ? null
            : Math.Round(decidedTimes.Average(d => (d.DecidedAt!.Value - d.SubmittedAt).TotalHours), 1);

        double? approvalRate = approved + rejected == 0
            ? null
            : Math.Round(approved * 100.0 / (approved + rejected), 1);

        var verificationStats = new VerificationStatsDto(
            pending + underReview + approved + rejected,
            pending, underReview, approved, rejected, overdue, unassignedInRegion,
            approvalRate, averageHours);

        var attention = await mine
            .Where(v => v.Status == VerificationStatus.Pending || v.Status == VerificationStatus.UnderReview)
            .OrderBy(v => v.DueDate)
            .Take(5)
            .Select(v => new
            {
                v.Id,
                v.ReferenceNumber,
                v.ItemName,
                v.FarmerName,
                v.Status,
                QuotationCount = v.Quotations.Count,
                v.DueDate
            })
            .ToListAsync();

        var needsAttention = attention
            .Select(v => new VerificationSummaryDto(
                v.Id, v.ReferenceNumber, v.ItemName, v.FarmerName, v.Status,
                v.QuotationCount, v.DueDate, v.DueDate < now))
            .ToList();

        // ----- Compliance monitoring -----
        var records = _db.ComplianceRecords.AsNoTracking().Where(c => c.OfficerId == officerId);

        var complianceCounts = await records
            .GroupBy(c => c.Status)
            .Select(g => new { Status = g.Key, Count = g.Count() })
            .ToListAsync();

        int ComplianceCountOf(ComplianceStatus s) => complianceCounts.FirstOrDefault(c => c.Status == s)?.Count ?? 0;

        var compliant = ComplianceCountOf(ComplianceStatus.Compliant);
        var underWatch = ComplianceCountOf(ComplianceStatus.UnderWatch);
        var nonCompliant = ComplianceCountOf(ComplianceStatus.NonCompliant);

        var reviewsDueCount = await records.CountAsync(c => c.NextReviewDate != null && c.NextReviewDate <= now);

        var dueRecords = await records
            .Where(c => c.NextReviewDate != null && c.NextReviewDate <= now)
            .OrderBy(c => c.NextReviewDate)
            .Take(5)
            .ToListAsync();

        var reviewsDue = dueRecords
            .Select(c => new ComplianceDueDto(
                c.Id, c.SubjectType, c.SubjectId, c.SubjectName, c.Standard, c.Status, c.NextReviewDate!.Value))
            .ToList();

        var complianceStats = new ComplianceStatsDto(
            compliant + underWatch + nonCompliant, compliant, underWatch, nonCompliant, reviewsDueCount);

        // ----- Recommendations -----
        var recs = _db.Recommendations.AsNoTracking().Where(r => r.OfficerId == officerId);
        var recTotal = await recs.CountAsync();
        var recHigh = await recs.CountAsync(r => r.Priority == RecommendationPriority.High);

        return new DashboardResponseDto(
            officer.Id, officer.FullName, officer.Region,
            verificationStats, complianceStats, new RecommendationStatsDto(recTotal, recHigh),
            needsAttention, reviewsDue);
    }

    public async Task<PlatformSummaryDto> GetPlatformSummaryAsync()
    {
        var now = DateTime.UtcNow;

        var totalOfficers = await _db.Officers.CountAsync();
        var activeOfficers = await _db.Officers.CountAsync(o => o.IsActive);

        var statusCounts = await _db.VerificationRequests
            .GroupBy(v => v.Status)
            .Select(g => new { Status = g.Key, Count = g.Count() })
            .ToListAsync();

        int CountOf(VerificationStatus s) => statusCounts.FirstOrDefault(c => c.Status == s)?.Count ?? 0;

        var pending = CountOf(VerificationStatus.Pending);
        var underReview = CountOf(VerificationStatus.UnderReview);
        var approved = CountOf(VerificationStatus.Approved);
        var rejected = CountOf(VerificationStatus.Rejected);

        var unassigned = await _db.VerificationRequests.CountAsync(v =>
            v.AssignedOfficerId == null &&
            (v.Status == VerificationStatus.Pending || v.Status == VerificationStatus.UnderReview));

        var overdue = await _db.VerificationRequests.CountAsync(v =>
            (v.Status == VerificationStatus.Pending || v.Status == VerificationStatus.UnderReview)
            && v.DueDate < now);

        double? approvalRate = approved + rejected == 0
            ? null
            : Math.Round(approved * 100.0 / (approved + rejected), 1);

        var nonCompliant = await _db.ComplianceRecords.CountAsync(c => c.Status == ComplianceStatus.NonCompliant);
        var reviewsDue = await _db.ComplianceRecords.CountAsync(c => c.NextReviewDate != null && c.NextReviewDate <= now);
        var failedCallbacks = await _db.VerificationRequests.CountAsync(v => v.CallbackStatus == CallbackStatus.Failed);

        return new PlatformSummaryDto(
            totalOfficers, activeOfficers,
            pending + underReview + approved + rejected,
            pending, underReview, approved, rejected,
            unassigned, overdue, approvalRate, nonCompliant, reviewsDue, failedCallbacks);
    }
}
