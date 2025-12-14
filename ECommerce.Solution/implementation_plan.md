# JWT Refresh Token Implementation Plan

## Overview
Implement production-grade JWT authentication with refresh tokens, token revocation, and secure token management.

## Current State
- ✅ Access token generation (7-day expiration)
- ✅ JWT validation middleware
- ✅ Role-based claims
- ❌ No refresh token
- ❌ No token revocation
- ❌ No token refresh endpoint

## Proposed Changes

### 1. Database Schema

#### New Table: RefreshTokens (Enhanced)
```sql
CREATE TABLE RefreshTokens (
    RefreshTokenId INT PRIMARY KEY IDENTITY,
    Token NVARCHAR(500) NOT NULL UNIQUE,
    UserId NVARCHAR(450) NOT NULL,
    ExpiresAt DATETIME2 NOT NULL,
    CreatedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    RevokedAt DATETIME2 NULL,
    ReplacedByToken NVARCHAR(500) NULL,
    IsRevoked BIT NOT NULL DEFAULT 0,
    IsUsed BIT NOT NULL DEFAULT 0,
    
    -- CRITICAL SECURITY ENHANCEMENTS
    IpAddress NVARCHAR(45) NULL,              -- IPv4/IPv6 support
    UserAgent NVARCHAR(500) NULL,             -- Browser/device fingerprint
    DeviceName NVARCHAR(100) NULL,            -- Optional device identifier
    LastUsedAt DATETIME2 NULL,                -- For sliding expiration
    
    CONSTRAINT FK_RefreshTokens_Users FOREIGN KEY (UserId) 
        REFERENCES AspNetUsers(Id) ON DELETE CASCADE
);

CREATE INDEX IX_RefreshTokens_Token ON RefreshTokens(Token);
CREATE INDEX IX_RefreshTokens_UserId ON RefreshTokens(UserId);
CREATE INDEX IX_RefreshTokens_UserId_IsActive ON RefreshTokens(UserId) 
    WHERE IsRevoked = 0 AND IsUsed = 0; -- Active tokens only
```

**Fields:**
- `Token`: Unique refresh token (128 bytes → Base64 = 172 chars)
- `UserId`: FK to ApplicationUser
- `ExpiresAt`: Refresh token expiration (30 days initial)
- `RevokedAt`: When token was revoked (null if active)
- `ReplacedByToken`: Token that replaced this one (rotation)
- `IsRevoked`: Quick check for revocation
- `IsUsed`: Prevent token reuse
- `IpAddress`: IP address when token created (security audit)
- `UserAgent`: Browser/device fingerprint (anomaly detection)
- `DeviceName`: Optional user-friendly device name
- `LastUsedAt`: Last refresh time (for sliding expiration)

---

### 2. Domain Layer

#### New Entity: `RefreshToken` (Enhanced)
**File:** `ECommerce.Domain/Entities/RefreshToken.cs`

```csharp
public class RefreshToken
{
    public int RefreshTokenId { get; set; }
    public string Token { get; set; } = string.Empty;
    public string UserId { get; set; } = string.Empty;
    public DateTime ExpiresAt { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? RevokedAt { get; set; }
    public string? ReplacedByToken { get; set; }
    public bool IsRevoked { get; set; }
    public bool IsUsed { get; set; }
    
    // CRITICAL SECURITY FIELDS
    public string? IpAddress { get; set; }
    public string? UserAgent { get; set; }
    public string? DeviceName { get; set; }
    public DateTime? LastUsedAt { get; set; }
    
    // Navigation
    public ApplicationUser User { get; set; } = null!;
    
    // Computed
    public bool IsActive => !IsRevoked && !IsUsed && DateTime.UtcNow < ExpiresAt;
}
```

---

### 3. Infrastructure Layer

#### A. EF Core Configuration
**File:** `ECommerce.Infrastructure/Data/EntityConfigurations.cs`

```csharp
public class RefreshTokenConfiguration : IEntityTypeConfiguration<RefreshToken>
{
    public void Configure(EntityTypeBuilder<RefreshToken> builder)
    {
        builder.HasKey(rt => rt.RefreshTokenId);
        
        builder.Property(rt => rt.Token)
            .IsRequired()
            .HasMaxLength(500);
        
        builder.HasIndex(rt => rt.Token)
            .IsUnique();
        
        builder.HasIndex(rt => rt.UserId);
        
        builder.HasOne(rt => rt.User)
            .WithMany()
            .HasForeignKey(rt => rt.UserId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
```

#### B. Repository Interface (Enhanced)
**File:** `ECommerce.Application/Interfaces/IRefreshTokenRepository.cs`

```csharp
public interface IRefreshTokenRepository
{
    Task<RefreshToken?> GetByTokenAsync(string token);
    Task<IEnumerable<RefreshToken>> GetActiveTokensByUserIdAsync(string userId);
    Task<int> GetActiveTokenCountAsync(string userId); // NEW: Device limit check
    Task AddAsync(RefreshToken refreshToken);
    Task UpdateAsync(RefreshToken refreshToken);
    Task RevokeAllUserTokensAsync(string userId);
    Task RevokeOldestTokenAsync(string userId); // NEW: Auto-revoke for device limit
}
```

#### C. Repository Implementation
**File:** `ECommerce.Infrastructure/Repositories/RefreshTokenRepository.cs`

```csharp
public class RefreshTokenRepository : IRefreshTokenRepository
{
    private readonly ECommerceDbContext _context;
    
    public async Task<RefreshToken?> GetByTokenAsync(string token)
    {
        return await _context.RefreshTokens
            .Include(rt => rt.User)
            .FirstOrDefaultAsync(rt => rt.Token == token);
    }
    
    public async Task<IEnumerable<RefreshToken>> GetActiveTokensByUserIdAsync(string userId)
    {
        return await _context.RefreshTokens
            .Where(rt => rt.UserId == userId && !rt.IsRevoked && !rt.IsUsed)
            .ToListAsync();
    }
    
    public async Task AddAsync(RefreshToken refreshToken)
    {
        await _context.RefreshTokens.AddAsync(refreshToken);
        await _context.SaveChangesAsync();
    }
    
    public async Task UpdateAsync(RefreshToken refreshToken)
    {
        _context.RefreshTokens.Update(refreshToken);
        await _context.SaveChangesAsync();
    }
    
    public async Task RevokeAllUserTokensAsync(string userId)
    {
        var tokens = await _context.RefreshTokens
            .Where(rt => rt.UserId == userId && !rt.IsRevoked)
            .ToListAsync();
        
        foreach (var token in tokens)
        {
            token.IsRevoked = true;
            token.RevokedAt = DateTime.UtcNow;
        }
        
        await _context.SaveChangesAsync();
    }
}
```

#### D. Enhanced JWT Service (Production-Grade)
**File:** `ECommerce.Infrastructure/Services/JwtService.cs`

**New Methods:**
```csharp
public interface IJwtService
{
    string GenerateAccessToken(ApplicationUser user, IList<string> roles);
    string GenerateRefreshToken(); // NEW: 128-byte cryptographic token
    Task<RefreshToken> CreateRefreshTokenAsync(string userId, string ipAddress, string userAgent, string? deviceName = null); // ENHANCED
    Task<bool> ValidateRefreshTokenAsync(string token, string ipAddress, string userAgent); // ENHANCED: Anomaly detection
    Task<RefreshToken> RefreshTokenAsync(string token, string ipAddress, string userAgent); // NEW: Atomic rotation
    Task RevokeRefreshTokenAsync(string token); // NEW
    Task RevokeAllUserTokensAsync(string userId); // NEW
}
```

**Implementation:**
```csharp
// CRITICAL: 128-byte token (172 chars Base64)
public string GenerateRefreshToken()
{
    var randomBytes = new byte[128]; // ENHANCED: 64 → 128 bytes
    using var rng = RandomNumberGenerator.Create();
    rng.GetBytes(randomBytes);
    return Convert.ToBase64String(randomBytes);
}

// CRITICAL: IP + UserAgent tracking
public async Task<RefreshToken> CreateRefreshTokenAsync(
    string userId, 
    string ipAddress, 
    string userAgent, 
    string? deviceName = null)
{
    // CRITICAL: Check device limit (max 5 active tokens)
    var activeCount = await _refreshTokenRepository.GetActiveTokenCountAsync(userId);
    if (activeCount >= 5)
    {
        // Auto-revoke oldest token
        await _refreshTokenRepository.RevokeOldestTokenAsync(userId);
    }
    
    var refreshToken = new RefreshToken
    {
        Token = GenerateRefreshToken(),
        UserId = userId,
        ExpiresAt = DateTime.UtcNow.AddDays(30),
        CreatedAt = DateTime.UtcNow,
        IpAddress = ipAddress,
        UserAgent = userAgent,
        DeviceName = deviceName,
        LastUsedAt = DateTime.UtcNow
    };
    
    await _refreshTokenRepository.AddAsync(refreshToken);
    return refreshToken;
}

// CRITICAL: IP/UserAgent validation + Sliding expiration
public async Task<bool> ValidateRefreshTokenAsync(
    string token, 
    string ipAddress, 
    string userAgent)
{
    var refreshToken = await _refreshTokenRepository.GetByTokenAsync(token);
    
    if (refreshToken == null) return false;
    if (refreshToken.IsRevoked) return false;
    if (refreshToken.IsUsed) return false;
    if (DateTime.UtcNow >= refreshToken.ExpiresAt) return false;
    
    // CRITICAL: Anomaly detection (optional - can be warning instead of rejection)
    if (refreshToken.IpAddress != ipAddress)
    {
        _logger.LogWarning("IP mismatch for token {Token}. Original: {Original}, Current: {Current}", 
            token, refreshToken.IpAddress, ipAddress);
        // Optional: return false; (strict mode)
    }
    
    if (refreshToken.UserAgent != userAgent)
    {
        _logger.LogWarning("UserAgent mismatch for token {Token}", token);
        // Optional: return false; (strict mode)
    }
    
    return true;
}

// CRITICAL: Atomic refresh with transaction
public async Task<RefreshToken> RefreshTokenAsync(
    string token, 
    string ipAddress, 
    string userAgent)
{
    // CRITICAL: Use transaction for atomic update
    using var transaction = await _context.Database.BeginTransactionAsync();
    
    try
    {
        var oldToken = await _refreshTokenRepository.GetByTokenAsync(token);
        if (oldToken == null)
            throw new SecurityException("Invalid refresh token");
        
        // Mark old token as used
        oldToken.IsUsed = true;
        oldToken.LastUsedAt = DateTime.UtcNow;
        
        // Generate new token
        var newToken = new RefreshToken
        {
            Token = GenerateRefreshToken(),
            UserId = oldToken.UserId,
            ExpiresAt = DateTime.UtcNow.AddDays(30), // SLIDING: Reset expiration
            CreatedAt = DateTime.UtcNow,
            IpAddress = ipAddress,
            UserAgent = userAgent,
            DeviceName = oldToken.DeviceName,
            LastUsedAt = DateTime.UtcNow
        };
        
        // Link tokens (rotation chain)
        oldToken.ReplacedByToken = newToken.Token;
        
        await _refreshTokenRepository.UpdateAsync(oldToken);
        await _refreshTokenRepository.AddAsync(newToken);
        
        await transaction.CommitAsync();
        
        return newToken;
    }
    catch
    {
        await transaction.RollbackAsync();
        throw;
    }
}
```

---

### 4. Application Layer

#### DTOs
**File:** `ECommerce.Application/DTOs/AuthDTOs.cs`

```csharp
public class LoginResponse
{
    public string UserId { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string AccessToken { get; set; } = string.Empty;
    public string RefreshToken { get; set; } = string.Empty; // NEW
    public DateTime AccessTokenExpiration { get; set; } // NEW
    public DateTime RefreshTokenExpiration { get; set; } // NEW
}

public class RefreshTokenRequest
{
    public string RefreshToken { get; set; } = string.Empty;
}

public class RevokeTokenRequest
{
    public string RefreshToken { get; set; } = string.Empty;
}
```

---

### 5. API Layer

#### Enhanced AuthController (Production-Grade Security)
**File:** `ECommerce.Api/Controllers/AuthController.cs`

**Helper Methods:**
```csharp
private string GetIpAddress()
{
    if (Request.Headers.ContainsKey("X-Forwarded-For"))
        return Request.Headers["X-Forwarded-For"].ToString();
    
    return HttpContext.Connection.RemoteIpAddress?.ToString() ?? "Unknown";
}

private string GetUserAgent()
{
    return Request.Headers["User-Agent"].ToString();
}

// CRITICAL: Set HttpOnly cookie for browser clients
private void SetRefreshTokenCookie(string refreshToken, DateTime expires)
{
    var cookieOptions = new CookieOptions
    {
        HttpOnly = true,        // Prevent XSS attacks
        Secure = true,          // HTTPS only
        SameSite = SameSiteMode.Strict, // CSRF protection
        Expires = expires
    };
    
    Response.Cookies.Append("refreshToken", refreshToken, cookieOptions);
}
```

**New Endpoints:**

```csharp
/// <summary>
/// Refresh access token (CRITICAL: Atomic transaction + IP tracking)
/// </summary>
[HttpPost("refresh")]
[ProducesResponseType(typeof(LoginResponse), StatusCodes.Status200OK)]
[ProducesResponseType(StatusCodes.Status401Unauthorized)]
public async Task<IActionResult> RefreshToken([FromBody] RefreshTokenRequest? request)
{
    // CRITICAL: Support both cookie and body
    var refreshToken = request?.RefreshToken ?? Request.Cookies["refreshToken"];
    
    if (string.IsNullOrEmpty(refreshToken))
        return Unauthorized(new { message = "Refresh token required" });
    
    var ipAddress = GetIpAddress();
    var userAgent = GetUserAgent();
    
    // CRITICAL: Validate with IP/UserAgent
    var isValid = await _jwtService.ValidateRefreshTokenAsync(refreshToken, ipAddress, userAgent);
    if (!isValid)
        return Unauthorized(new { message = "Invalid or expired refresh token" });
    
    // CRITICAL: Atomic refresh (transaction inside)
    var newRefreshToken = await _jwtService.RefreshTokenAsync(refreshToken, ipAddress, userAgent);
    
    // Get user and generate new access token
    var user = await _userManager.FindByIdAsync(newRefreshToken.UserId);
    var roles = await _userManager.GetRolesAsync(user);
    var newAccessToken = _jwtService.GenerateAccessToken(user, roles);
    
    // CRITICAL: Set HttpOnly cookie
    SetRefreshTokenCookie(newRefreshToken.Token, newRefreshToken.ExpiresAt);
    
    return Ok(new LoginResponse
    {
        UserId = user.Id,
        Email = user.Email,
        AccessToken = newAccessToken,
        RefreshToken = newRefreshToken.Token, // Also return in body for mobile
        AccessTokenExpiration = DateTime.UtcNow.AddDays(7),
        RefreshTokenExpiration = newRefreshToken.ExpiresAt
    });
}

/// <summary>
/// Revoke a refresh token
/// </summary>
[HttpPost("revoke")]
[Authorize]
[ProducesResponseType(StatusCodes.Status200OK)]
public async Task<IActionResult> RevokeToken([FromBody] RevokeTokenRequest? request)
{
    var refreshToken = request?.RefreshToken ?? Request.Cookies["refreshToken"];
    
    if (!string.IsNullOrEmpty(refreshToken))
    {
        await _jwtService.RevokeRefreshTokenAsync(refreshToken);
        Response.Cookies.Delete("refreshToken");
    }
    
    return Ok(new { message = "Token revoked successfully" });
}

/// <summary>
/// Revoke all refresh tokens (logout from all devices)
/// </summary>
[HttpPost("revoke-all")]
[Authorize]
[ProducesResponseType(StatusCodes.Status200OK)]
public async Task<IActionResult> RevokeAllTokens()
{
    var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
    await _jwtService.RevokeAllUserTokensAsync(userId);
    Response.Cookies.Delete("refreshToken");
    return Ok(new { message = "All tokens revoked successfully" });
}
```

**Update Login Endpoint:**
```csharp
[HttpPost("login")]
public async Task<IActionResult> Login([FromBody] LoginRequest request)
{
    // ... existing validation ...
    
    var ipAddress = GetIpAddress();
    var userAgent = GetUserAgent();
    
    var accessToken = _jwtService.GenerateAccessToken(user, roles);
    var refreshToken = await _jwtService.CreateRefreshTokenAsync(
        user.Id, 
        ipAddress, 
        userAgent, 
        request.DeviceName); // ENHANCED
    
    // CRITICAL: Set HttpOnly cookie
    SetRefreshTokenCookie(refreshToken.Token, refreshToken.ExpiresAt);
    
    return Ok(new LoginResponse
    {
        UserId = user.Id,
        Email = user.Email,
        AccessToken = accessToken,
        RefreshToken = refreshToken.Token, // Also in body for mobile
        AccessTokenExpiration = DateTime.UtcNow.AddDays(7),
        RefreshTokenExpiration = refreshToken.ExpiresAt
    });
}
```

---

## Verification Plan

### 1. Database Migration
```bash
dotnet ef migrations add AddRefreshTokens --project ECommerce.Infrastructure --startup-project ECommerce.Api
dotnet ef database update --project ECommerce.Infrastructure --startup-project ECommerce.Api
```

### 2. Manual Testing (Swagger)

**Test 1: Login with Refresh Token**
```
POST /api/auth/login
{
  "email": "demo@example.com",
  "password": "Demo123"
}

Expected Response:
{
  "accessToken": "eyJhbGc...",
  "refreshToken": "base64-encoded-token",
  "accessTokenExpiration": "2025-12-12T...",
  "refreshTokenExpiration": "2026-01-04T..."
}
```

**Test 2: Refresh Access Token**
```
POST /api/auth/refresh
{
  "refreshToken": "base64-encoded-token"
}

Expected Response:
{
  "accessToken": "new-eyJhbGc...",
  "refreshToken": "new-base64-token",
  ...
}
```

**Test 3: Revoke Token**
```
POST /api/auth/revoke
Authorization: Bearer {access-token}
{
  "refreshToken": "base64-encoded-token"
}

Expected: 200 OK
```

**Test 4: Use Revoked Token**
```
POST /api/auth/refresh
{
  "refreshToken": "revoked-token"
}

Expected: 401 Unauthorized
```

### 3. Integration Tests

Create `RefreshTokenIntegrationTests.cs`:
- Test token generation
- Test token validation
- Test token refresh flow
- Test token revocation
- Test expired token rejection

---

## Security Considerations

✅ **Token Rotation:** Old refresh token marked as used, new one generated  
✅ **Revocation:** Tokens can be revoked (logout, security breach)  
✅ **Expiration:** Refresh tokens expire after 30 days  
✅ **One-Time Use:** Refresh tokens can only be used once  
✅ **Cryptographic Random:** Refresh tokens use `RandomNumberGenerator`  
✅ **Database Storage:** All tokens tracked in database  

---

## Configuration

**appsettings.json:**
```json
{
  "JwtSettings": {
    "SecretKey": "your-secret-key-min-32-chars",
    "Issuer": "ECommerceAPI",
    "Audience": "ECommerceClient",
    "AccessTokenExpirationDays": 7,
    "RefreshTokenExpirationDays": 30
  }
}
```

---

## Rollout Plan

1. Create database migration
2. Update `IJwtService` interface
3. Implement `RefreshTokenRepository`
4. Update `JwtService` with refresh token methods
5. Update `AuthController` (login, refresh, revoke endpoints)
6. Update DTOs
7. Test in Swagger
8. Create integration tests
9. Update API documentation

---

## Success Criteria

- ✅ Refresh tokens stored in database
- ✅ Access token can be refreshed without re-login
- ✅ Tokens can be revoked
- ✅ Expired tokens rejected
- ✅ Used tokens cannot be reused
- ✅ All tests passing
- ✅ Swagger documentation updated
