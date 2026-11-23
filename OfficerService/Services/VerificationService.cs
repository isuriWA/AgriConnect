using Microsoft.EntityFrameworkCore;
using OfficerService.Data;
using OfficerService.DTOs;
using OfficerService.Entities;
using OfficerService.IServices;

namespace OfficerService.Services;

/// <summary>
/// Verification workflow for a supply request and the quotations received for it:
/// Pending -> (officer starts review) -> UnderReview -> (compliance checks, rate each quotation)
/// -> Approved / Rejected -> decision is sent to the Farmer service.
/// </summary>
public class VerificationService : IVerificationService
{
    /// <summary>Officers are expected to decide within this many days of submission.</summary>
    private const int ReviewDeadlineDays = 3;

    private readonly OfficerDbContext _db;
    private readonly IFarmerCallbackClient _farmerCallback;

    public VerificationService(OfficerDbContext db, IFarmerCallbackClient farmerCallback)
    {
        _db = db;
        _farmerCallback = farmerCallback;
    }

    // ---------- Queries ----------

    public async Task<IEnumerable<VerificationResponseDto>> GetAllAsync(
        int? assignedOfficerId, bool unassignedOnly, VerificationStatus? status,
        int? farmerId, bool? overdueOnly)
    {
        var query = BaseQuery().AsNoTracking();

        if (assignedOfficerId.HasValue) query = query.Where(v => v.AssignedOfficerId == assignedOfficerId.Value);
        if (unassignedOnly) query = query.Where(v => v.AssignedOfficerId == null);
        if (status.HasValue) query = query.Where(v => v.Status == status.Value);
        if (farmerId.HasValue) query = query.Where(v => v.FarmerId == farmerId.Value);

        if (overdueOnly == true)
        {
            var now = DateTime.UtcNow;
            query = query.Where(v =>
                (v.Status == VerificationStatus.Pending || v.Status == VerificationStatus.UnderReview)
                && v.DueDate < now);
        }

        var items = await query.OrderBy(v => v.DueDate).ToListAsync();
        return items.Select(ToDto);
    }

    public async Task<VerificationResponseDto> GetByIdAsync(int id)
    {
        var verification = await BaseQuery().AsNoTracking().FirstOrDefaultAsync(v => v.Id == id)
            ?? throw new KeyNotFoundException($"Verification {id} was not found.");
        return ToDto(verification);
    }

    public async Task<VerificationResponseDto> GetBySourceRequestIdAsync(int sourceRequestId)
    {
        var verification = await BaseQuery().AsNoTracking()
                .FirstOrDefaultAsync(v => v.SourceRequestId == sourceRequestId)
            ?? throw new KeyNotFoundException(
                $"No verification exists for supply request {sourceRequestId}.");
        return ToDto(verification);
    }

    // ---------- Intake from the Farmer and Supplier services ----------

    public async Task<VerificationResponseDto> CreateAsync(CreateVerificationDto dto)
    {
        if (await _db.VerificationRequests.AnyAsync(v => v.SourceRequestId == dto.SourceRequestId))
            throw new ConflictException(
                $"Supply request {dto.SourceRequestId} has already been forwarded for verification.");

        var region = dto.Region.Trim();

        // Auto-assign: active officer in the same region with the fewest open verifications.
        var officer = await _db.Officers
            .Where(o => o.IsActive && o.Region == region)
            .OrderBy(o => _db.VerificationRequests.Count(v =>
                v.AssignedOfficerId == o.Id &&
                (v.Status == VerificationStatus.Pending || v.Status == VerificationStatus.UnderReview)))
            .ThenBy(o => o.Id)
            .FirstOrDefaultAsync();

        var now = DateTime.UtcNow;
        var verification = new VerificationRequest
        {
            // Temporary unique value; replaced with VR-yyyy-nnnnn once the Id is known.
            ReferenceNumber = Guid.NewGuid().ToString("N")[..24],
            SourceRequestId = dto.SourceRequestId,
            FarmerId = dto.FarmerId,
            FarmerName = dto.FarmerName.Trim(),
            Region = region,
            ItemName = dto.ItemName.Trim(),
            Quantity = dto.Quantity,
            DeliveryLocation = dto.DeliveryLocation.Trim(),
            PreferredDate = dto.PreferredDate,
            AssignedOfficerId = officer?.Id,
            AssignedOfficer = officer,
            SubmittedAt = now,
            DueDate = now.AddDays(ReviewDeadlineDays)
        };

        _db.VerificationRequests.Add(verification);
        await _db.SaveChangesAsync();

        verification.ReferenceNumber = $"VR-{now:yyyy}-{verification.Id:D5}";
        await _db.SaveChangesAsync();

        return ToDto(verification);
    }

    public async Task<VerificationResponseDto> AddQuotationAsync(int sourceRequestId, AddQuotationDto dto)
    {
        var verification = await BaseQuery().FirstOrDefaultAsync(v => v.SourceRequestId == sourceRequestId)
            ?? throw new KeyNotFoundException(
                $"No verification exists for supply request {sourceRequestId}. Forward the request first.");

        if (IsClosed(verification))
            throw new InvalidOperationException(
                $"Supply request {sourceRequestId} has already been {verification.Status.ToString().ToLowerInvariant()}; it no longer accepts quotations.");

        if (await _db.Quotations.AnyAsync(q => q.SourceQuotationId == dto.SourceQuotationId))
            throw new ConflictException($"Quotation {dto.SourceQuotationId} has already been forwarded.");

        if (verification.Quotations.Any(q => q.SupplierId == dto.SupplierId))
            throw new ConflictException("This supplier has already quoted for this request.");

        verification.Quotations.Add(new Quotation
        {
            SourceQuotationId = dto.SourceQuotationId,
            SupplierId = dto.SupplierId,
            SupplierName = dto.SupplierName.Trim(),
            UnitPrice = dto.UnitPrice,
            StockQuantity = dto.StockQuantity,
            EstimatedDeliveryDate = dto.EstimatedDeliveryDate,
            Notes = dto.Notes?.Trim()
        });

        await _db.SaveChangesAsync();
        return ToDto(verification);
    }

    // ---------- Assignment ----------

    public async Task<VerificationResponseDto> ClaimAsync(int id, int officerId)
    {
        var verification = await LoadAsync(id);

        if (verification.AssignedOfficerId is not null)
            throw new InvalidOperationException("This verification is already assigned to an officer.");

        var officer = await GetActiveOfficerAsync(officerId);

        verification.AssignedOfficerId = officer.Id;
        verification.AssignedOfficer = officer;

        await _db.SaveChangesAsync();
        return ToDto(verification);
    }

    public async Task<VerificationResponseDto> HandoverAsync(int id, int currentOfficerId, HandoverDto dto)
    {
        var verification = await LoadAsync(id);
        EnsureAssignedOfficer(verification, currentOfficerId);

        if (IsClosed(verification))
            throw new InvalidOperationException("A decided verification cannot be handed over.");

        if (dto.OfficerId == currentOfficerId)
            throw new InvalidOperationException("Choose a different officer to hand this over to.");

        var officer = await GetActiveOfficerAsync(dto.OfficerId);

        verification.AssignedOfficerId = officer.Id;
        verification.AssignedOfficer = officer;

        // The new officer starts the review afresh (existing checks and ratings are kept).
        verification.Status = VerificationStatus.Pending;
        verification.ReviewStartedAt = null;

        await _db.SaveChangesAsync();
        return ToDto(verification);
    }

    // ---------- Officer workflow ----------

    public async Task<VerificationResponseDto> StartReviewAsync(int id, int officerId)
    {
        var verification = await LoadAsync(id);
        EnsureAssignedOfficer(verification, officerId);

        if (verification.Status != VerificationStatus.Pending)
            throw new InvalidOperationException(
                $"Only pending verifications can be started. This one is {verification.Status}.");

        verification.Status = VerificationStatus.UnderReview;
        verification.ReviewStartedAt = DateTime.UtcNow;

        await _db.SaveChangesAsync();
        return ToDto(verification);
    }

    public async Task<VerificationResponseDto> AddCheckAsync(int id, int officerId, AddComplianceCheckDto dto)
    {
        var verification = await LoadAsync(id);
        EnsureAssignedOfficer(verification, officerId);
        EnsureUnderReview(verification);

        Quotation? quotation = null;
        if (dto.QuotationId.HasValue)
        {
            quotation = verification.Quotations.FirstOrDefault(q => q.Id == dto.QuotationId.Value)
                ?? throw new KeyNotFoundException(
                    $"Quotation {dto.QuotationId.Value} does not belong to this verification.");
        }

        verification.Checks.Add(new ComplianceCheck
        {
            QuotationId = quotation?.Id,
            Criterion = dto.Criterion.Trim(),
            IsCompliant = dto.IsCompliant,
            Remarks = dto.Remarks?.Trim(),
            CheckedByOfficerId = officerId
        });

        // A failed check invalidates an earlier "suitable" rating: the officer must re-rate it.
        if (!dto.IsCompliant && quotation is not null && IsSuitableRating(quotation.Suitability))
        {
            quotation.Suitability = SuitabilityStatus.NotAssessed;
            quotation.AssessedAt = null;
        }

        await _db.SaveChangesAsync();
        return ToDto(verification);
    }

    public async Task<VerificationResponseDto> RemoveCheckAsync(int id, int checkId, int officerId)
    {
        var verification = await LoadAsync(id);
        EnsureAssignedOfficer(verification, officerId);
        EnsureUnderReview(verification);

        var check = verification.Checks.FirstOrDefault(c => c.Id == checkId)
            ?? throw new KeyNotFoundException($"Compliance check {checkId} was not found on this verification.");

        _db.ComplianceChecks.Remove(check);
        await _db.SaveChangesAsync();
        return ToDto(verification);
    }

    public async Task<VerificationResponseDto> AssessQuotationAsync(
        int id, int quotationId, int officerId, AssessQuotationDto dto)
    {
        var verification = await LoadAsync(id);
        EnsureAssignedOfficer(verification, officerId);
        EnsureUnderReview(verification);

        var quotation = verification.Quotations.FirstOrDefault(q => q.Id == quotationId)
            ?? throw new KeyNotFoundException($"Quotation {quotationId} does not belong to this verification.");

        if (dto.Suitability == SuitabilityStatus.NotAssessed)
            throw new InvalidOperationException(
                "Choose Suitable, ConditionallySuitable or NotSuitable.");

        if (IsSuitableRating(dto.Suitability) &&
            verification.Checks.Any(c => c.QuotationId == quotationId && !c.IsCompliant))
            throw new InvalidOperationException(
                "This quotation has a failed compliance check, so it cannot be rated suitable. Rate it NotSuitable or remove the failed check.");

        quotation.Suitability = dto.Suitability;
        quotation.SuitabilityNotes = dto.Notes?.Trim();
        quotation.AssessedAt = DateTime.UtcNow;

        await _db.SaveChangesAsync();
        return ToDto(verification);
    }

    public async Task<VerificationResponseDto> DecideAsync(int id, int officerId, DecideVerificationDto dto)
    {
        var verification = await LoadAsync(id);
        EnsureAssignedOfficer(verification, officerId);
        EnsureUnderReview(verification);

        var remarks = dto.Remarks?.Trim();

        if (dto.Approve)
        {
            var requestChecks = verification.Checks.Where(c => c.QuotationId == null).ToList();

            if (requestChecks.Count == 0)
                throw new InvalidOperationException(
                    "Record at least one compliance check on the request before approving.");

            if (requestChecks.Any(c => !c.IsCompliant))
                throw new InvalidOperationException(
                    "Cannot approve while a compliance check on the request has failed. Reject it, or remove the failed check if it was a mistake.");

            if (verification.Quotations.Count == 0)
                throw new InvalidOperationException("There are no quotations to verify yet.");

            if (verification.Quotations.Any(q => q.Suitability == SuitabilityStatus.NotAssessed))
                throw new InvalidOperationException("Rate every quotation's suitability before approving.");

            if (!verification.Quotations.Any(q => IsSuitableRating(q.Suitability)))
                throw new InvalidOperationException(
                    "At least one quotation must be Suitable or ConditionallySuitable to approve the request.");
        }
        else if (string.IsNullOrWhiteSpace(remarks))
        {
            throw new InvalidOperationException("Remarks are required when rejecting.");
        }

        verification.Status = dto.Approve ? VerificationStatus.Approved : VerificationStatus.Rejected;
        verification.DecisionRemarks = remarks;
        verification.DecidedAt = DateTime.UtcNow;
        await _db.SaveChangesAsync();

        // Tell the Farmer service. If this fails the decision still stands and can be resent.
        await SendCallbackAsync(verification);

        return ToDto(verification);
    }

    public async Task<VerificationResponseDto> ResendCallbackAsync(int id, int officerId)
    {
        var verification = await LoadAsync(id);
        EnsureAssignedOfficer(verification, officerId);

        if (!IsClosed(verification))
            throw new InvalidOperationException("The decision can only be sent after the request is approved or rejected.");

        if (verification.CallbackStatus == CallbackStatus.Sent)
            throw new InvalidOperationException("The decision has already been delivered to the Farmer service.");

        await SendCallbackAsync(verification);
        return ToDto(verification);
    }

    // ---------- Helpers ----------

    private async Task SendCallbackAsync(VerificationRequest v)
    {
        var approved = v.Status == VerificationStatus.Approved
            ? v.Quotations
                .Where(q => IsSuitableRating(q.Suitability))
                .OrderBy(q => q.UnitPrice)
                .Select(q => new ApprovedQuotationCallback(
                    q.SourceQuotationId, q.SupplierId, q.SupplierName, q.UnitPrice,
                    Math.Round(q.UnitPrice * v.Quantity, 2), q.StockQuantity, q.EstimatedDeliveryDate,
                    q.Suitability.ToString(), q.SuitabilityNotes))
                .ToList()
            : new List<ApprovedQuotationCallback>();

        var payload = new VerificationDecisionCallback(
            v.SourceRequestId, v.ReferenceNumber, v.Status.ToString(), v.DecisionRemarks,
            v.DecidedAt ?? DateTime.UtcNow, v.AssignedOfficerId ?? 0,
            v.AssignedOfficer?.FullName ?? string.Empty, approved);

        var result = await _farmerCallback.SendDecisionAsync(payload);

        v.CallbackAttempts++;
        v.LastCallbackAt = DateTime.UtcNow;
        v.CallbackStatus = result.Success ? CallbackStatus.Sent : CallbackStatus.Failed;
        v.LastCallbackError = result.Success ? null : result.Error;

        await _db.SaveChangesAsync();
    }

    private IQueryable<VerificationRequest> BaseQuery() =>
        _db.VerificationRequests
            .Include(v => v.AssignedOfficer)
            .Include(v => v.Quotations)
            .Include(v => v.Checks)
            .AsSplitQuery();

    private async Task<VerificationRequest> LoadAsync(int id) =>
        await BaseQuery().FirstOrDefaultAsync(v => v.Id == id)
        ?? throw new KeyNotFoundException($"Verification {id} was not found.");

    private async Task<Officer> GetActiveOfficerAsync(int officerId)
    {
        var officer = await _db.Officers.FindAsync(officerId)
            ?? throw new KeyNotFoundException($"Officer {officerId} was not found.");

        if (!officer.IsActive)
            throw new InvalidOperationException("Cannot assign work to an inactive officer.");

        return officer;
    }

    private static bool IsClosed(VerificationRequest v) =>
        v.Status is VerificationStatus.Approved or VerificationStatus.Rejected;

    private static bool IsSuitableRating(SuitabilityStatus s) =>
        s is SuitabilityStatus.Suitable or SuitabilityStatus.ConditionallySuitable;

    private static void EnsureAssignedOfficer(VerificationRequest v, int officerId)
    {
        if (v.AssignedOfficerId is null)
            throw new InvalidOperationException("This verification has no assigned officer yet. Claim it first.");

        if (v.AssignedOfficerId != officerId)
            throw new ForbiddenException("Only the assigned officer can work on this verification.");
    }

    private static void EnsureUnderReview(VerificationRequest v)
    {
        if (v.Status != VerificationStatus.UnderReview)
            throw new InvalidOperationException(
                $"Start the review first. This verification is {v.Status}.");
    }

    private static VerificationResponseDto ToDto(VerificationRequest v)
    {
        var isOpen = v.Status is VerificationStatus.Pending or VerificationStatus.UnderReview;

        var quotations = v.Quotations
            .OrderBy(q => q.UnitPrice)
            .Select(q => new QuotationResponseDto(
                q.Id, q.SourceQuotationId, q.SupplierId, q.SupplierName, q.UnitPrice,
                Math.Round(q.UnitPrice * v.Quantity, 2), q.StockQuantity,
                q.StockQuantity >= v.Quantity, q.EstimatedDeliveryDate,
                q.EstimatedDeliveryDate.Date <= v.PreferredDate.Date,
                q.Notes, q.SubmittedAt, q.Suitability, q.SuitabilityNotes, q.AssessedAt))
            .ToList();

        var checks = v.Checks
            .OrderBy(c => c.CheckedAt)
            .Select(c => new ComplianceCheckResponseDto(
                c.Id, c.QuotationId, c.Criterion, c.IsCompliant, c.Remarks, c.CheckedByOfficerId, c.CheckedAt))
            .ToList();

        return new VerificationResponseDto(
            v.Id, v.ReferenceNumber, v.SourceRequestId, v.FarmerId, v.FarmerName, v.Region,
            v.ItemName, v.Quantity, v.DeliveryLocation, v.PreferredDate, v.Status,
            v.AssignedOfficerId, v.AssignedOfficer?.FullName, v.DecisionRemarks,
            v.SubmittedAt, v.DueDate, isOpen && v.DueDate < DateTime.UtcNow,
            v.ReviewStartedAt, v.DecidedAt,
            v.CallbackStatus, v.CallbackAttempts, v.LastCallbackError,
            quotations, checks);
    }
}
