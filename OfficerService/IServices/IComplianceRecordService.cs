using OfficerService.DTOs;
using OfficerService.Entities;

namespace OfficerService.IServices;

public interface IComplianceRecordService
{
    Task<IEnumerable<ComplianceRecordResponseDto>> GetAllAsync(
        int? officerId, ComplianceSubjectType? subjectType, int? subjectId,
        ComplianceStatus? status, bool? reviewDue);

    Task<ComplianceRecordResponseDto> GetByIdAsync(int id);
    Task<ComplianceRecordResponseDto> CreateAsync(int officerId, CreateComplianceRecordDto dto);
    Task<ComplianceRecordResponseDto> UpdateAsync(int id, int officerId, UpdateComplianceRecordDto dto);
}
