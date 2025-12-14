using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using ECommerce.Application.Interfaces.Repositories;
using ECommerce.Domain.Entities;
using ECommerce.Infrastructure.Data;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;

namespace ECommerce.Api.Middleware;

public class AdminAuditMiddleware
{
    private readonly RequestDelegate _next;

    public AdminAuditMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        // Only audit admin requests
        if (!context.Request.Path.StartsWithSegments("/api/v1/Admin") || 
            context.Request.Method == "GET")
        {
            await _next(context);
            return;
        }

        // Capture request body for POST/PUT if needed (simplified here)
        // We'll proceed with the request first to see if it succeeds
        
        await _next(context);

        // Only log successful modifications
        if (context.Response.StatusCode >= 200 && context.Response.StatusCode < 300)
        {
            try
            {
                var userId = context.User.FindFirstValue(ClaimTypes.NameIdentifier);
                var email = context.User.FindFirstValue(ClaimTypes.Email);

                // If user claim is missing (e.g. login endpoint), skip
                if (string.IsNullOrEmpty(userId))
                    return;

                // Create scope to resolve scoped services
                using var scope = context.RequestServices.CreateScope();
                var dbContext = scope.ServiceProvider.GetRequiredService<ECommerceDbContext>();

                var auditLog = new AdminAuditLog
                {
                    AdminUserId = userId,
                    AdminEmail = email,
                    Action = context.Request.Method,
                    EntityType = ExtractEntityType(context.Request.Path),
                    EntityId = ExtractEntityId(context.Request.Path),
                    IpAddress = context.Connection.RemoteIpAddress?.ToString(),
                    UserAgent = context.Request.Headers["User-Agent"].ToString(),
                    Timestamp = DateTime.UtcNow,
                    NewValues = context.Request.Path // Storing path as summary for now
                };

                dbContext.AdminAuditLogs.Add(auditLog);
                await dbContext.SaveChangesAsync();
            }
            catch
            {
                // Fail silently, don't block the request
            }
        }
    }

    private string ExtractEntityType(PathString path)
    {
        var segments = path.Value?.Split('/');
        if (segments != null && segments.Length > 4)
        {
            return segments[4]; // /api/v1/Admin/{Entity}
        }
        return "Unknown";
    }

    private string ExtractEntityId(PathString path)
    {
        var segments = path.Value?.Split('/');
        if (segments != null && segments.Length > 5)
        {
            return segments[5]; // /api/v1/Admin/{Entity}/{Id}
        }
        return string.Empty;
    }
}
