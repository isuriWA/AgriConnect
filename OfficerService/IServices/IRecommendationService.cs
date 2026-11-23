using OfficerService.DTOs;
using OfficerService.Entities;

namespace OfficerService.IServices;

public interface IRecommendationService
{
    Task<IEnumerable<RecommendationResponseDto>> GetAllAsync(
        int? officerId, int? verificationRequestId, int? farmerId, int? supplierId, RecommendationPriority? priority);

    Task<RecommendationResponseDto> GetByIdAsync(int id);
    Task<RecommendationResponseDto> CreateAsync(int officerId, CreateRecommendationDto dto);
    Task<RecommendationResponseDto> UpdateAsync(int id, int officerId, UpdateRecommendationDto dto);
    Task DeleteAsync(int id, int officerId);
}
