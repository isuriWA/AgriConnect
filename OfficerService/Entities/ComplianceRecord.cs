namespace OfficerService.Entities;

public class ComplianceRecord
{
    public int Id { get; set; }

    public int OfficerId { get; set; }
    public Officer? Officer { get; set; }

    public ComplianceSubjectType SubjectType { get; set; }
    public int SubjectId { get; set; }

    public string SubjectName { get; set; } = string.Empty;
    public string Standard { get; set; } = string.Empty;
    public ComplianceStatus Status { get; set; } = ComplianceStatus.Compliant;
    public string Findings { get; set; } = string.Empty;

    public string? CorrectiveAction { get; set; }

    public DateTime InspectedAt { get; set; } = DateTime.UtcNow;
    public DateTime? NextReviewDate { get; set; }
}