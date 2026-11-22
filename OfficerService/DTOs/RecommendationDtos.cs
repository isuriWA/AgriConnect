using System.ComponentModel.DataAnnotations;
using OfficerService.Entities;

namespace OfficerService.DTOs;

public class CreateRecommendationDto
{
    /// <summary>Optional link to a verification. If set and no target is given, that verification's farmer is the target.</summary>
    [Range(1, int.MaxValue)]
    public int? VerificationRequestId { get; set; }

    [Range(1, int.MaxValue)]
    public int? TargetFarmerId { get; set; }

    [Range(1, int.MaxValue)]
    public int? TargetSupplierId { get; set; }

    [Required, StringLength(150)]
    public string Title { get; set; } = string.Empty;

    [Required, StringLength(4000)]
    public string Details { get; set; } = string.Empty;

    public RecommendationPriority Priority { get; set; } = RecommendationPriority.Medium;
}

public class UpdateRecommendationDto
{
    [Required, StringLength(150)]
    public string Title { get; set; } = string.Empty;

    [Required, StringLength(4000)]
    public string Details { get; set; } = string.Empty;

    public RecommendationPriority Priority { get; set; } = RecommendationPriority.Medium;
}

public record RecommendationResponseDto(
    int Id,
    int OfficerId,
    string OfficerName,
    int? VerificationRequestId,
    int? TargetFarmerId,
    int? TargetSupplierId,
    string Title,
    string Details,
    RecommendationPriority Priority,
    DateTime CreatedAt);
