using OfficerService.DTOs;

namespace OfficerService.IServices;

/// <summary>Tells the Farmer service what the officer decided.</summary>
public interface IFarmerCallbackClient
{
    Task<CallbackResult> SendDecisionAsync(VerificationDecisionCallback payload, CancellationToken cancellationToken = default);
}

public record CallbackResult(bool Success, string? Error);
