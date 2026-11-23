using OfficerService.DTOs;
using OfficerService.Entities;

namespace OfficerService.IServices;

public interface IVerificationService
{
    // Queries
    Task<IEnumerable<VerificationResponseDto>> GetAllAsync(
        int? assignedOfficerId, bool unassignedOnly, VerificationStatus? status,
        int? farmerId, bool? overdueOnly);

    Task<VerificationResponseDto> GetByIdAsync(int id);
    Task<VerificationResponseDto> GetBySourceRequestIdAsync(int sourceRequestId);

    // Intake from the Farmer and Supplier services
    Task<VerificationResponseDto> CreateAsync(CreateVerificationDto dto);
    Task<VerificationResponseDto> AddQuotationAsync(int sourceRequestId, AddQuotationDto dto);

    // Assignment
    Task<VerificationResponseDto> ClaimAsync(int id, int officerId);
    Task<VerificationResponseDto> HandoverAsync(int id, int currentOfficerId, HandoverDto dto);

    // Officer workflow
    Task<VerificationResponseDto> StartReviewAsync(int id, int officerId);
    Task<VerificationResponseDto> AddCheckAsync(int id, int officerId, AddComplianceCheckDto dto);
    Task<VerificationResponseDto> RemoveCheckAsync(int id, int checkId, int officerId);
    Task<VerificationResponseDto> AssessQuotationAsync(int id, int quotationId, int officerId, AssessQuotationDto dto);
    Task<VerificationResponseDto> DecideAsync(int id, int officerId, DecideVerificationDto dto);
    Task<VerificationResponseDto> ResendCallbackAsync(int id, int officerId);
}
