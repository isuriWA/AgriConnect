using System.ComponentModel.DataAnnotations;
using OfficerService.Entities;

namespace OfficerService.DTOs;

/// <summary>Sent by the Farmer service when a farmer submits a supply request.</summary>
public class CreateVerificationDto
{
    /// <summary>The supply request's id in the Farmer service.</summary>
    [Range(1, int.MaxValue)]
    public int SourceRequestId { get; set; }

    [Range(1, int.MaxValue)]
    public int FarmerId { get; set; }

    [Required, StringLength(150)]
    public string FarmerName { get; set; } = string.Empty;

    /// <summary>Region (district) of the delivery location. Decides which officer receives it.</summary>
    [Required, StringLength(100)]
    public string Region { get; set; } = string.Empty;

    [Required, StringLength(200)]
    public string ItemName { get; set; } = string.Empty;

    [Range(0.01, 1_000_000_000)]
    public decimal Quantity { get; set; }

    [Required, StringLength(300)]
    public string DeliveryLocation { get; set; } = string.Empty;

    [Required]
    public DateTime PreferredDate { get; set; }
}

/// <summary>Sent by the Supplier service when a supplier quotes for a request.</summary>
public class AddQuotationDto
{
    /// <summary>The quotation's id in the Supplier service.</summary>
    [Range(1, int.MaxValue)]
    public int SourceQuotationId { get; set; }

    [Range(1, int.MaxValue)]
    public int SupplierId { get; set; }

    [Required, StringLength(150)]
    public string SupplierName { get; set; } = string.Empty;

    [Range(0.01, 1_000_000_000)]
    public decimal UnitPrice { get; set; }

    [Range(0, 1_000_000_000)]
    public decimal StockQuantity { get; set; }

    [Required]
    public DateTime EstimatedDeliveryDate { get; set; }

    [StringLength(1000)]
    public string? Notes { get; set; }
}

public class HandoverDto
{
    [Range(1, int.MaxValue)]
    public int OfficerId { get; set; }
}

public class AddComplianceCheckDto
{
    /// <summary>Leave empty to check the request itself; set it to check one quotation.</summary>
    [Range(1, int.MaxValue)]
    public int? QuotationId { get; set; }

    [Required, StringLength(300)]
    public string Criterion { get; set; } = string.Empty;

    public bool IsCompliant { get; set; }

    [StringLength(1000)]
    public string? Remarks { get; set; }
}

public class AssessQuotationDto
{
    /// <summary>Suitable, ConditionallySuitable or NotSuitable.</summary>
    public SuitabilityStatus Suitability { get; set; }

    [StringLength(1000)]
    public string? Notes { get; set; }
}

public class DecideVerificationDto
{
    /// <summary>True = approve, false = reject.</summary>
    public bool Approve { get; set; }

    /// <summary>Required when rejecting.</summary>
    [StringLength(1000)]
    public string? Remarks { get; set; }
}

public record ComplianceCheckResponseDto(
    int Id,
    int? QuotationId,
    string Criterion,
    bool IsCompliant,
    string? Remarks,
    int CheckedByOfficerId,
    DateTime CheckedAt);

public record QuotationResponseDto(
    int Id,
    int SourceQuotationId,
    int SupplierId,
    string SupplierName,
    decimal UnitPrice,
    decimal TotalPrice,
    decimal StockQuantity,
    bool StockSufficient,
    DateTime EstimatedDeliveryDate,
    bool DeliversByPreferredDate,
    string? Notes,
    DateTime SubmittedAt,
    SuitabilityStatus Suitability,
    string? SuitabilityNotes,
    DateTime? AssessedAt);

public record VerificationResponseDto(
    int Id,
    string ReferenceNumber,
    int SourceRequestId,
    int FarmerId,
    string FarmerName,
    string Region,
    string ItemName,
    decimal Quantity,
    string DeliveryLocation,
    DateTime PreferredDate,
    VerificationStatus Status,
    int? AssignedOfficerId,
    string? AssignedOfficerName,
    string? DecisionRemarks,
    DateTime SubmittedAt,
    DateTime DueDate,
    bool IsOverdue,
    DateTime? ReviewStartedAt,
    DateTime? DecidedAt,
    CallbackStatus CallbackStatus,
    int CallbackAttempts,
    string? LastCallbackError,
    IReadOnlyList<QuotationResponseDto> Quotations,
    IReadOnlyList<ComplianceCheckResponseDto> Checks);
