using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OfficerService.DTOs;
using OfficerService.Entities;
using OfficerService.Extensions;
using OfficerService.IServices;
using OfficerService.Security;

namespace OfficerService.Controllers;

[ApiController]
[Route("api/[controller]")]
[Tags("Verifications")]
[Authorize]
public class VerificationsController : ControllerBase
{
    private readonly IVerificationService _verifications;

    public VerificationsController(IVerificationService verifications)
    {
        _verifications = verifications;
    }

    // =====================================================================
    // Called by the Farmer and Supplier services (server to server, X-Api-Key)
    // =====================================================================

    /// <summary>Farmer service: forward a new supply request. It is auto-assigned to an officer in the delivery region.</summary>
    [HttpPost]
    [AllowAnonymous]
    [ServiceKeyAuth]
    public async Task<ActionResult<VerificationResponseDto>> Create(CreateVerificationDto dto)
    {
        var created = await _verifications.CreateAsync(dto);
        return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
    }

    /// <summary>Supplier service: forward a supplier's quotation for a supply request.</summary>
    [HttpPost("by-request/{sourceRequestId:int}/quotations")]
    [AllowAnonymous]
    [ServiceKeyAuth]
    public async Task<ActionResult<VerificationResponseDto>> AddQuotation(int sourceRequestId, AddQuotationDto dto) =>
        Ok(await _verifications.AddQuotationAsync(sourceRequestId, dto));

    /// <summary>Farmer or Supplier service: check the verification status of a supply request.</summary>
    [HttpGet("by-request/{sourceRequestId:int}")]
    [AllowAnonymous]
    [ServiceKeyAuth]
    public async Task<ActionResult<VerificationResponseDto>> GetBySourceRequest(int sourceRequestId) =>
        Ok(await _verifications.GetBySourceRequestIdAsync(sourceRequestId));

    // =====================================================================
    // Officer screens (JWT)
    // =====================================================================

    /// <summary>List verifications, oldest deadline first. Use mine=true for your own queue, unassigned=true for ones nobody has claimed.</summary>
    [HttpGet]
    public async Task<ActionResult<IEnumerable<VerificationResponseDto>>> GetAll(
        [FromQuery] bool mine, [FromQuery] bool unassigned, [FromQuery] VerificationStatus? status,
        [FromQuery] int? farmerId, [FromQuery] bool? overdueOnly)
    {
        int? officerId = mine ? User.GetOfficerId() : null;
        return Ok(await _verifications.GetAllAsync(officerId, unassigned, status, farmerId, overdueOnly));
    }

    /// <summary>One verification: the request, all quotations and the compliance checklist.</summary>
    [HttpGet("{id:int}")]
    public async Task<ActionResult<VerificationResponseDto>> GetById(int id) =>
        Ok(await _verifications.GetByIdAsync(id));

    /// <summary>Take an unassigned verification (when no officer covered that region).</summary>
    [HttpPatch("{id:int}/claim")]
    public async Task<ActionResult<VerificationResponseDto>> Claim(int id) =>
        Ok(await _verifications.ClaimAsync(id, User.GetOfficerId()));

    /// <summary>Hand your verification over to another officer.</summary>
    [HttpPatch("{id:int}/handover")]
    public async Task<ActionResult<VerificationResponseDto>> Handover(int id, HandoverDto dto) =>
        Ok(await _verifications.HandoverAsync(id, User.GetOfficerId(), dto));

    /// <summary>Step 1: start the review (Pending to UnderReview).</summary>
    [HttpPatch("{id:int}/start-review")]
    public async Task<ActionResult<VerificationResponseDto>> StartReview(int id) =>
        Ok(await _verifications.StartReviewAsync(id, User.GetOfficerId()));

    /// <summary>Step 2: record a compliance check on the request, or on one quotation (set quotationId).</summary>
    [HttpPost("{id:int}/checks")]
    public async Task<ActionResult<VerificationResponseDto>> AddCheck(int id, AddComplianceCheckDto dto) =>
        Ok(await _verifications.AddCheckAsync(id, User.GetOfficerId(), dto));

    /// <summary>Remove a compliance check entered by mistake.</summary>
    [HttpDelete("{id:int}/checks/{checkId:int}")]
    public async Task<ActionResult<VerificationResponseDto>> RemoveCheck(int id, int checkId) =>
        Ok(await _verifications.RemoveCheckAsync(id, checkId, User.GetOfficerId()));

    /// <summary>Step 3: verify a quotation's suitability (Suitable, ConditionallySuitable, NotSuitable).</summary>
    [HttpPatch("{id:int}/quotations/{quotationId:int}/suitability")]
    public async Task<ActionResult<VerificationResponseDto>> AssessQuotation(
        int id, int quotationId, AssessQuotationDto dto) =>
        Ok(await _verifications.AssessQuotationAsync(id, quotationId, User.GetOfficerId(), dto));

    /// <summary>Step 4: approve or reject the request. The decision is then sent to the Farmer service.</summary>
    [HttpPatch("{id:int}/decision")]
    public async Task<ActionResult<VerificationResponseDto>> Decide(int id, DecideVerificationDto dto) =>
        Ok(await _verifications.DecideAsync(id, User.GetOfficerId(), dto));

    /// <summary>Retry sending the decision to the Farmer service after a failed delivery.</summary>
    [HttpPost("{id:int}/resend-decision")]
    public async Task<ActionResult<VerificationResponseDto>> ResendDecision(int id) =>
        Ok(await _verifications.ResendCallbackAsync(id, User.GetOfficerId()));
}
