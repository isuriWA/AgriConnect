using Microsoft.EntityFrameworkCore;
using OfficerService.Data;
using OfficerService.DTOs;
using OfficerService.Entities;
using OfficerService.IServices;

namespace OfficerService.Services;

public class RecommendationService : IRecommendationService
{
    private readonly OfficerDbContext _db;

    public RecommendationService(OfficerDbContext db)
    {
        _db = db;
    }

    public async Task<IEnumerable<RecommendationResponseDto>> GetAllAsync(
        int? officerId, int? verificationRequestId, int? farmerId, int? supplierId,
        RecommendationPriority? priority)
    {
        var query = _db.Recommendations.AsNoTracking().Include(r => r.Officer).AsQueryable();

        if (officerId.HasValue) query = query.Where(r => r.OfficerId == officerId.Value);
        if (verificationRequestId.HasValue) query = query.Where(r => r.VerificationRequestId == verificationRequestId.Value);
        if (farmerId.HasValue) query = query.Where(r => r.TargetFarmerId == farmerId.Value);
        if (supplierId.HasValue) query = query.Where(r => r.TargetSupplierId == supplierId.Value);
        if (priority.HasValue) query = query.Where(r => r.Priority == priority.Value);

        var items = await query.OrderByDescending(r => r.CreatedAt).ToListAsync();
        return items.Select(ToDto);
    }

    public async Task<RecommendationResponseDto> GetByIdAsync(int id)
    {
        var item = await _db.Recommendations.AsNoTracking().Include(r => r.Officer)
                .FirstOrDefaultAsync(r => r.Id == id)
            ?? throw new KeyNotFoundException($"Recommendation {id} was not found.");
        return ToDto(item);
    }

    public async Task<RecommendationResponseDto> CreateAsync(int officerId, CreateRecommendationDto dto)
    {
        var officer = await _db.Officers.FindAsync(officerId)
            ?? throw new KeyNotFoundException($"Officer {officerId} was not found.");

        if (!officer.IsActive)
            throw new InvalidOperationException("An inactive officer cannot create recommendations.");

        var targetFarmerId = dto.TargetFarmerId;
        var targetSupplierId = dto.TargetSupplierId;

        if (dto.VerificationRequestId.HasValue)
        {
            var verification = await _db.VerificationRequests.AsNoTracking()
                    .FirstOrDefaultAsync(v => v.Id == dto.VerificationRequestId.Value)
                ?? throw new KeyNotFoundException(
                    $"Verification {dto.VerificationRequestId.Value} was not found.");

            // Default the audience to the farmer who made the request.
            if (targetFarmerId is null && targetSupplierId is null)
                targetFarmerId = verification.FarmerId;
        }

        if (targetFarmerId is null && targetSupplierId is null)
            throw new InvalidOperationException(
                "Give a verification, a target farmer or a target supplier for this recommendation.");

        var recommendation = new Recommendation
        {
            OfficerId = officer.Id,
            Officer = officer,
            VerificationRequestId = dto.VerificationRequestId,
            TargetFarmerId = targetFarmerId,
            TargetSupplierId = targetSupplierId,
            Title = dto.Title.Trim(),
            Details = dto.Details.Trim(),
            Priority = dto.Priority
        };

        _db.Recommendations.Add(recommendation);
        await _db.SaveChangesAsync();
        return ToDto(recommendation);
    }

    public async Task<RecommendationResponseDto> UpdateAsync(int id, int officerId, UpdateRecommendationDto dto)
    {
        var item = await _db.Recommendations.Include(r => r.Officer).FirstOrDefaultAsync(r => r.Id == id)
            ?? throw new KeyNotFoundException($"Recommendation {id} was not found.");

        if (item.OfficerId != officerId)
            throw new ForbiddenException("You can only edit your own recommendations.");

        item.Title = dto.Title.Trim();
        item.Details = dto.Details.Trim();
        item.Priority = dto.Priority;

        await _db.SaveChangesAsync();
        return ToDto(item);
    }

    public async Task DeleteAsync(int id, int officerId)
    {
        var item = await _db.Recommendations.FindAsync(id)
            ?? throw new KeyNotFoundException($"Recommendation {id} was not found.");

        if (item.OfficerId != officerId)
            throw new ForbiddenException("You can only delete your own recommendations.");

        _db.Recommendations.Remove(item);
        await _db.SaveChangesAsync();
    }

    private static RecommendationResponseDto ToDto(Recommendation r) =>
        new(r.Id, r.OfficerId, r.Officer?.FullName ?? string.Empty, r.VerificationRequestId,
            r.TargetFarmerId, r.TargetSupplierId, r.Title, r.Details, r.Priority, r.CreatedAt);
}
