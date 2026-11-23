using System.Security.Claims;

namespace OfficerService.Extensions;

public static class ClaimsPrincipalExtensions
{
    public static int GetOfficerId(this ClaimsPrincipal principal)
    {
        ArgumentNullException.ThrowIfNull(principal);

        var value = principal.FindFirst(ClaimTypes.NameIdentifier)?.Value
            ?? principal.FindFirst("sub")?.Value;

        if (!int.TryParse(value, out var officerId) || officerId <= 0)
            throw new InvalidOperationException("The authenticated user does not have a valid officer identifier claim.");

        return officerId;
    }
}
