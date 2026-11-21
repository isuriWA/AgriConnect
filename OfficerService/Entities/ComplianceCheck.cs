namespace OfficerService.Entities;

public class ComplianceCheck
{
    public int Id { get; set; }

    public int VerificationRequestId { get; set; }
    public VerificationRequest? VerificationRequest { get; set; }

    public int? QuotationId { get; set; }
    public Quotation? Quotation { get; set; }

    public string Criterion { get; set; } = string.Empty;

    public bool IsCompliant { get; set; }
    public string? Remarks { get; set; }

    public int CheckedByOfficerId { get; set; }
    public DateTime CheckedAt { get; set; } = DateTime.UtcNow;
}