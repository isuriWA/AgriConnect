namespace OfficerService.Entities;

public class Quotation
{
    public int Id { get; set; }

    public int VerificationRequestId { get; set; }
    public VerificationRequest? VerificationRequest { get; set; }

    public int SourceQuotationId { get; set; }

    public int SupplierId { get; set; }
    public string SupplierName { get; set; } = string.Empty;

    public decimal UnitPrice { get; set; }
    public decimal StockQuantity { get; set; }
    public DateTime EstimatedDeliveryDate { get; set; }
    public string? Notes { get; set; }
    public DateTime SubmittedAt { get; set; } = DateTime.UtcNow;

    public SuitabilityStatus Suitability { get; set; } = SuitabilityStatus.NotAssessed;
    public string? SuitabilityNotes { get; set; }
    public DateTime? AssessedAt { get; set; }
}