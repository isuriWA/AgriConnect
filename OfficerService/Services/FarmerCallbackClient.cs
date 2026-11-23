using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.Extensions.Options;
using OfficerService.DTOs;
using OfficerService.IServices;
using OfficerService.Settings;

namespace OfficerService.Services;

/// <summary>
/// Posts the officer's decision to the Farmer service. A failure never throws: the result tells
/// the caller whether it worked so the decision itself is not lost.
/// </summary>
public class FarmerCallbackClient : IFarmerCallbackClient
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    private readonly HttpClient _http;
    private readonly FarmerServiceOptions _options;
    private readonly ILogger<FarmerCallbackClient> _logger;

    public FarmerCallbackClient(
        HttpClient http, IOptions<FarmerServiceOptions> options, ILogger<FarmerCallbackClient> logger)
    {
        _http = http;
        _options = options.Value;
        _logger = logger;
    }

    public async Task<CallbackResult> SendDecisionAsync(
        VerificationDecisionCallback payload, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(_options.BaseUrl))
            return new CallbackResult(false, "ExternalServices:FarmerService:BaseUrl is not configured.");

        var path = _options.DecisionPath.Replace("{sourceRequestId}", payload.SourceRequestId.ToString());
        var url = _options.BaseUrl.TrimEnd('/') + "/" + path.TrimStart('/');

        try
        {
            using var request = new HttpRequestMessage(HttpMethod.Post, url)
            {
                Content = JsonContent.Create(payload, mediaType: null, options: JsonOptions)
            };

            if (!string.IsNullOrWhiteSpace(_options.ApiKey))
                request.Headers.Add("X-Api-Key", _options.ApiKey);

            using var response = await _http.SendAsync(request, cancellationToken);

            if (response.IsSuccessStatusCode)
                return new CallbackResult(true, null);

            var body = await response.Content.ReadAsStringAsync(cancellationToken);
            if (body.Length > 200) body = body[..200];

            _logger.LogWarning("Farmer service rejected the decision callback for request {RequestId}: {Status} {Body}",
                payload.SourceRequestId, (int)response.StatusCode, body);

            return new CallbackResult(false, $"Farmer service returned {(int)response.StatusCode}: {body}");
        }
        catch (Exception ex) when (ex is HttpRequestException or TaskCanceledException)
        {
            _logger.LogWarning(ex, "Could not reach the Farmer service for request {RequestId}", payload.SourceRequestId);
            return new CallbackResult(false, $"Could not reach the Farmer service: {ex.Message}");
        }
    }
}
