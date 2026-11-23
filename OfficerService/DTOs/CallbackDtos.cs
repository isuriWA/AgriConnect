namespace OfficerService.DTOs;

/// <summary>
/// Body POSTed to the Farmer service when the officer approves or rejects a request.
/// The Farmer service must expose an endpoint that accepts this (see INTEGRATION.md).
/// </summary>
public record VerificationDecisionCallback(
    int SourceRequestId,
    string ReferenceNumber,
    string Decision,                 // "Approved" or "Rejected"
    string? Remarks,
    DateTime DecidedAt,
    int OfficerId,
    string OfficerName,
    IReadOnlyList<ApprovedQuotationCallback> ApprovedQuotations);

/// <summary>A quotation the officer found suitable; the farmer compares these. Empty when rejected.</summary>
public record ApprovedQuotationCallback(
    int SourceQuotationId,
    int SupplierId,
    string SupplierName,
    decimal UnitPrice,
    decimal TotalPrice,
    decimal StockQuantity,
    DateTime EstimatedDeliveryDate,
    string Suitability,              // "Suitable" or "ConditionallySuitable"
    string? SuitabilityNotes);
