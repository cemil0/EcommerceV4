# Rate Limiting Implementation Plan

## Overview
Implement production-grade rate limiting using ASP.NET Core 7.0+ built-in features to protect API from DDOS attacks and abuse.

## Proposed Policies

### 1. Global API Limit
- **Limit:** 100 requests/minute per IP
- **Window:** Fixed (1 minute)
- **Applies to:** All endpoints (default)

### 2. Auth Endpoints (Strict)
- **Limit:** 5 requests/minute per IP
- **Applies to:** `/api/auth/login`, `/api/auth/register`
- **Reason:** Brute force protection

### 3. Refresh Token (Moderate)
- **Limit:** 10 requests/minute per IP
- **Applies to:** `/api/auth/refresh`

### 4. Product Endpoints (Relaxed)
- **Limit:** 60 requests/minute per IP
- **Window:** Sliding (6 segments)
- **Applies to:** `/api/products/*`

## Implementation

### Program.cs Configuration
```csharp
builder.Services.AddRateLimiter(options =>
{
    options.AddFixedWindowLimiter("GlobalApiLimit", opt =>
    {
        opt.PermitLimit = 100;
        opt.Window = TimeSpan.FromMinutes(1);
        opt.QueueLimit = 5;
    });

    options.AddFixedWindowLimiter("AuthLimit", opt =>
    {
        opt.PermitLimit = 5;
        opt.Window = TimeSpan.FromMinutes(1);
        opt.QueueLimit = 0;
    });

    options.OnRejected = async (context, ct) =>
    {
        context.HttpContext.Response.StatusCode = 429;
        await context.HttpContext.Response.WriteAsJsonAsync(new
        {
            error = "Too many requests. Please try again later."
        }, ct);
    };
});

app.UseRateLimiter(); // After UseRouting, before UseAuthorization
```

### Controller Attributes
```csharp
[EnableRateLimiting("AuthLimit")]
public async Task<ActionResult> Login([FromBody] LoginRequest request)
```

## Success Criteria
- ✅ 429 responses when limit exceeded
- ✅ Retry-After header present
- ✅ Auth endpoints limited to 5 req/min
- ✅ No performance degradation
