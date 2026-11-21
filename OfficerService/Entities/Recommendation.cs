namespace OfficerService.Entities;

public class Recommendation
{
    public int Id { get; set; }

    public int OfficerId { get; set; }
    public Officer? Officer { get; set; }

    public int? VerificationRequestId { get; set; }
    public VerificationRequest? VerificationRequest { get; set; }

    public int? TargetFarmerId { get; set; }
    public int? TargetSupplierId { get; set; }

    public string Title { get; set; } = string.Empty;
    public string Details { get; set; } = string.Empty;
    public RecommendationPriority Priority { get; set; } = RecommendationPriority.Medium;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}