using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OfficerService.DTOs;
using OfficerService.Entities;
using OfficerService.Extensions;
using OfficerService.IServices;

namespace OfficerService.Controllers;

[ApiController]
[Route("api/[controller]")]
[Tags("Compliance Monitoring")]
[Authorize]
public class ComplianceRecordsController : ControllerBase
{
    private readonly IComplianceRecordService _records;

    public ComplianceRecordsController(IComplianceRecordService records)
    {
        _records = records;
    }

    /// <summary>List inspection records. mine=true shows yours; reviewDue=true shows those whose next review date has passed.</summary>
    [HttpGet]
    public async Task<ActionResult<IEnumerable<ComplianceRecordResponseDto>>> GetAll(
        [FromQuery] bool mine, [FromQuery] ComplianceSubjectType? subjectType,
        [FromQuery] int? subjectId, [FromQuery] ComplianceStatus? status, [FromQuery] bool? reviewDue)
    {
        int? officerId = mine ? User.GetOfficerId() : null;
        return Ok(await _records.GetAllAsync(officerId, subjectType, subjectId, status, reviewDue));
    }

    /// <summary>Get one inspection record.</summary>
    [HttpGet("{id:int}")]
    public async Task<ActionResult<ComplianceRecordResponseDto>> GetById(int id) =>
        Ok(await _records.GetByIdAsync(id));

    /// <summary>Record an inspection of a farmer or supplier against a standard.</summary>
    [HttpPost]
    public async Task<ActionResult<ComplianceRecordResponseDto>> Create(CreateComplianceRecordDto dto)
    {
        var created = await _records.CreateAsync(User.GetOfficerId(), dto);
        return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
    }

    /// <summary>Re-inspect: update status, findings, corrective action and the next review date.</summary>
    [HttpPut("{id:int}")]
    public async Task<ActionResult<ComplianceRecordResponseDto>> Update(int id, UpdateComplianceRecordDto dto) =>
        Ok(await _records.UpdateAsync(id, User.GetOfficerId(), dto));
}
