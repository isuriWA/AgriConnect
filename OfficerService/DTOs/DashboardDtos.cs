using OfficerService.Entities;

namespace OfficerService.DTOs;

public record VerificationStatsDto(
    int Total,
    int Pending,
    int UnderReview,
    int Approved,
    int Rejected,
    int Overdue,
    int UnassignedInMyRegion,
    double? ApprovalRatePercent,
    double? AverageDecisionHours);

public record ComplianceStatsDto(
    int TotalRecords,
    int Compliant,
    int UnderWatch,
    int NonCompliant,
    int ReviewsDue);

public record RecommendationStatsDto(int Total, int HighPriority);

public record VerificationSummaryDto(
    int Id,
    string ReferenceNumber,
    string ItemName,
    string FarmerName,
    VerificationStatus Status,
    int QuotationCount,
    DateTime DueDate,
    bool IsOverdue);

public record ComplianceDueDto(
    int Id,
    ComplianceSubjectType SubjectType,
    int SubjectId,
    string SubjectName,
    string Standard,
    ComplianceStatus Status,
    DateTime NextReviewDate);

/// <summary>Everything the officer dashboard screen needs in a single call.</summary>
public record DashboardResponseDto(
    int OfficerId,
    string OfficerName,
    string Region,
    VerificationStatsDto Verifications,
    ComplianceStatsDto Compliance,
    RecommendationStatsDto Recommendations,
    IReadOnlyList<VerificationSummaryDto> NeedsAttention,
    IReadOnlyList<ComplianceDueDto> ReviewsDue);

/// <summary>Platform-wide numbers for the Admin service's monitoring and analytics.</summary>
public record PlatformSummaryDto(
    int TotalOfficers,
    int ActiveOfficers,
    int TotalVerifications,
    int Pending,
    int UnderReview,
    int Approved,
    int Rejected,
    int Unassigned,
    int Overdue,
    double? ApprovalRatePercent,
    int NonCompliantRecords,
    int ComplianceReviewsDue,
    int FailedCallbacks);
