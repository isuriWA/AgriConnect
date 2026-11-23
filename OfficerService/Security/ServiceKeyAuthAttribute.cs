using System.Security.Cryptography;
using System.Text;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.DependencyInjection;

namespace OfficerService.Security;

[AttributeUsage(AttributeTargets.Method)]
public sealed class ServiceKeyAuthAttribute : Attribute, IAsyncAuthorizationFilter
{
    public Task OnAuthorizationAsync(AuthorizationFilterContext context)
    {
        var services = context.HttpContext.RequestServices;
        var configuration = services.GetRequiredService<IConfiguration>();
        var logger = services.GetRequiredService<ILogger<ServiceKeyAuthAttribute>>();
        var expectedKey = configuration["OfficerService:ServiceKey"];

        if (string.IsNullOrWhiteSpace(expectedKey))
        {
            logger.LogError("OfficerService:ServiceKey is not configured.");
            context.Result = new StatusCodeResult(StatusCodes.Status503ServiceUnavailable);
            return Task.CompletedTask;
        }

        var suppliedKey = context.HttpContext.Request.Headers["X-Api-Key"].ToString();
        var expectedBytes = Encoding.UTF8.GetBytes(expectedKey);
        var suppliedBytes = Encoding.UTF8.GetBytes(suppliedKey);

        if (suppliedBytes.Length == 0 || !CryptographicOperations.FixedTimeEquals(expectedBytes, suppliedBytes))
            context.Result = new UnauthorizedResult();

        return Task.CompletedTask;
    }
}
