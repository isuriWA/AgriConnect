using System.ComponentModel.DataAnnotations;
using OfficerService.Entities;

namespace OfficerService.DTOs;

public class CreateComplianceRecordDto
{
    public ComplianceSubjectType SubjectType { get; set; }

    /// <summary>Farmer id or supplier id in their own service.</summary>
    [Range(1, int.MaxValue)]
    public int SubjectId { get; set; }

    [Required, StringLength(150)]
    public string SubjectName { get; set; } = string.Empty;

    [Required, StringLength(200)]
    public string Standard { get; set; } = string.Empty;

    public ComplianceStatus Status { get; set; } = ComplianceStatus.Compliant;

    [Required, StringLength(2000)]
    public string Findings { get; set; } = string.Empty;

    /// <summary>Required when Status is UnderWatch or NonCompliant.</summary>
    [StringLength(1000)]
    public string? CorrectiveAction { get; set; }

    /// <summary>Defaults to now.</summary>
    public DateTime? InspectedAt { get; set; }

    public DateTime? NextReviewDate { get; set; }
}

/// <summary>Re-inspection of an existing record. Sets InspectedAt to now.</summary>
public class UpdateComplianceRecordDto
{
    public ComplianceStatus Status { get; set; }

    [Required, StringLength(2000)]
    public string Findings { get; set; } = string.Empty;

    [StringLength(1000)]
    public string? CorrectiveAction { get; set; }

    public DateTime? NextReviewDate { get; set; }
}

public record ComplianceRecordResponseDto(
    int Id,
    int OfficerId,
    string OfficerName,
    ComplianceSubjectType SubjectType,
    int SubjectId,
    string SubjectName,
    string Standard,
    ComplianceStatus Status,
    string Findings,
    string? CorrectiveAction,
    DateTime InspectedAt,
    DateTime? NextReviewDate,
    bool IsReviewDue);
