namespace OfficerService.Entities;

public class VerificationRequest
{
    public int Id { get; set; }
    public string ReferenceNumber { get; set; } = string.Empty;
    public int SourceRequestId { get; set; }

    public int FarmerId { get; set; }
    public string FarmerName { get; set; } = string.Empty;
    public string Region { get; set; } = string.Empty;

    public string ItemName { get; set; } = string.Empty;
    public decimal Quantity { get; set; }
    public string DeliveryLocation { get; set; } = string.Empty;
    public DateTime PreferredDate { get; set; }

    public VerificationStatus Status { get; set; } = VerificationStatus.Pending;

    public int? AssignedOfficerId { get; set; }
    public Officer? AssignedOfficer { get; set; }

    public string? DecisionRemarks { get; set; }

    public DateTime SubmittedAt { get; set; } = DateTime.UtcNow;
    public DateTime DueDate { get; set; }
    public DateTime? ReviewStartedAt { get; set; }
    public DateTime? DecidedAt { get; set; }

    public CallbackStatus CallbackStatus { get; set; } = CallbackStatus.NotSent;
    public int CallbackAttempts { get; set; }
    public DateTime? LastCallbackAt { get; set; }
    public string? LastCallbackError { get; set; }

    public ICollection<Quotation> Quotations { get; set; } = new List<Quotation>();
    public ICollection<ComplianceCheck> Checks { get; set; } = new List<ComplianceCheck>();
    public ICollection<Recommendation> Recommendations { get; set; } = new List<Recommendation>();
}