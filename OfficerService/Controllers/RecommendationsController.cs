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
[Tags("Recommendations")]
[Authorize]
public class RecommendationsController : ControllerBase
{
    private readonly IRecommendationService _recommendations;

    public RecommendationsController(IRecommendationService recommendations)
    {
        _recommendations = recommendations;
    }

    /// <summary>List recommendations. Use mine=true for the ones you wrote.</summary>
    [HttpGet]
    public async Task<ActionResult<IEnumerable<RecommendationResponseDto>>> GetAll(
        [FromQuery] bool mine, [FromQuery] int? verificationRequestId,
        [FromQuery] int? farmerId, [FromQuery] int? supplierId,
        [FromQuery] RecommendationPriority? priority)
    {
        int? officerId = mine ? User.GetOfficerId() : null;
        return Ok(await _recommendations.GetAllAsync(officerId, verificationRequestId, farmerId, supplierId, priority));
    }

    /// <summary>Get one recommendation by id.</summary>
    [HttpGet("{id:int}")]
    public async Task<ActionResult<RecommendationResponseDto>> GetById(int id) =>
        Ok(await _recommendations.GetByIdAsync(id));

    /// <summary>Give a recommendation to a farmer and/or supplier, optionally tied to a verification.</summary>
    [HttpPost]
    public async Task<ActionResult<RecommendationResponseDto>> Create(CreateRecommendationDto dto)
    {
        var created = await _recommendations.CreateAsync(User.GetOfficerId(), dto);
        return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
    }

    /// <summary>Edit one of your recommendations.</summary>
    [HttpPut("{id:int}")]
    public async Task<ActionResult<RecommendationResponseDto>> Update(int id, UpdateRecommendationDto dto) =>
        Ok(await _recommendations.UpdateAsync(id, User.GetOfficerId(), dto));

    /// <summary>Delete one of your recommendations.</summary>
    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        await _recommendations.DeleteAsync(id, User.GetOfficerId());
        return NoContent();
    }

    /// <summary>Farmer service: recommendations addressed to a farmer, newest first. Needs the X-Api-Key header.</summary>
    [HttpGet("for-farmer/{farmerId:int}")]
    [AllowAnonymous]
    [ServiceKeyAuth]
    public async Task<ActionResult<IEnumerable<RecommendationResponseDto>>> ForFarmer(int farmerId) =>
        Ok(await _recommendations.GetAllAsync(null, null, farmerId, null, null));

    /// <summary>Supplier service: recommendations addressed to a supplier, newest first. Needs the X-Api-Key header.</summary>
    [HttpGet("for-supplier/{supplierId:int}")]
    [AllowAnonymous]
    [ServiceKeyAuth]
    public async Task<ActionResult<IEnumerable<RecommendationResponseDto>>> ForSupplier(int supplierId) =>
        Ok(await _recommendations.GetAllAsync(null, null, null, supplierId, null));
}
