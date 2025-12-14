using System.Security.Claims;

namespace ECommerce.Api.Extensions;

public static class ClaimsPrincipalExtensions
{
    public static int? GetCustomerId(this ClaimsPrincipal principal)
    {
        var customerIdClaim = principal.FindFirst("CustomerId");
        if (customerIdClaim != null && int.TryParse(customerIdClaim.Value, out var customerId))
        {
            return customerId;
        }
        return null;
    }

    public static string? GetUserId(this ClaimsPrincipal principal)
    {
        return principal.FindFirst(ClaimTypes.NameIdentifier)?.Value;
    }

    public static string? GetEmail(this ClaimsPrincipal principal)
    {
        return principal.FindFirst(ClaimTypes.Email)?.Value;
    }
}
