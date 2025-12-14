using ECommerce.Application.DTOs;
using ECommerce.Application.Interfaces;
using ECommerce.Application.Interfaces.Services;
using ECommerce.Domain.Entities;
using ECommerce.Domain.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.EntityFrameworkCore;
using Swashbuckle.AspNetCore.Annotations;
using System.Security.Claims;

namespace ECommerce.Api.Controllers;

[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/[controller]")]
public class AuthController : ControllerBase
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly SignInManager<ApplicationUser> _signInManager;
    private readonly IJwtService _jwtService;
    private readonly IRefreshTokenRepository _refreshTokenRepository;
    private readonly IUnitOfWork _unitOfWork;

    public AuthController(
        UserManager<ApplicationUser> userManager,
        SignInManager<ApplicationUser> signInManager,
        IJwtService jwtService,
        IRefreshTokenRepository refreshTokenRepository,
        IUnitOfWork unitOfWork)
    {
        _userManager = userManager;
        _signInManager = signInManager;
        _jwtService = jwtService;
        _refreshTokenRepository = refreshTokenRepository;
        _unitOfWork = unitOfWork;
    }

    // Helper Methods
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

    private void SetRefreshTokenCookie(string refreshToken, DateTime expires)
    {
        var cookieOptions = new CookieOptions
        {
            HttpOnly = true,
            Secure = true,
            SameSite = SameSiteMode.Strict,
            Expires = expires
        };

        Response.Cookies.Append("refreshToken", refreshToken, cookieOptions);
    }

    /// <summary>
    /// Register a new user
    /// </summary>
    [HttpPost("register")]
    [EnableRateLimiting("Auth")] // 5 req/min - Brute force protection
    [ProducesResponseType(typeof(LoginResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [SwaggerOperation(Summary = "Register user", Description = "Register a new user and return JWT tokens")]
    public async Task<ActionResult<LoginResponse>> Register([FromBody] RegisterRequest request)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        if (request.Password != request.ConfirmPassword)
            return BadRequest(new { error = "Passwords do not match" });

        var user = new ApplicationUser
        {
            UserName = request.Email,
            Email = request.Email,
            FirstName = request.FirstName,
            LastName = request.LastName,
            EmailConfirmed = false
        };

        var result = await _userManager.CreateAsync(user, request.Password);

        if (!result.Succeeded)
            return BadRequest(new { errors = result.Errors.Select(e => e.Description) });

        await _userManager.AddToRoleAsync(user, "Customer");

        // AUTO-CREATE CUSTOMER RECORD
        var customer = new Customer
        {
            ApplicationUserId = user.Id,
            CustomerType = CustomerType.B2C,
            FirstName = request.FirstName,
            LastName = request.LastName,
            Email = request.Email,
            Phone = null, // Optional: can be added later
            IsActive = true,
            IsEmailVerified = false,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
        await _unitOfWork.Customers.AddAsync(customer);
        await _unitOfWork.SaveChangesAsync();

        var roles = await _userManager.GetRolesAsync(user);
        var accessToken = _jwtService.GenerateToken(user.Id, user.Email!, roles, customer.CustomerId);
        
        var ipAddress = GetIpAddress();
        var userAgent = GetUserAgent();
        var refreshToken = await _jwtService.CreateRefreshTokenAsync(user.Id, ipAddress, userAgent, request.DeviceName);

        SetRefreshTokenCookie(refreshToken.Token, refreshToken.ExpiresAt);

        return Ok(new LoginResponse
        {
            AccessToken = accessToken,
            RefreshToken = refreshToken.Token,
            AccessTokenExpiration = DateTime.UtcNow.AddDays(7),
            RefreshTokenExpiration = refreshToken.ExpiresAt,
            Email = user.Email!,
            FirstName = user.FirstName,
            LastName = user.LastName
        });
    }

    /// <summary>
    /// Login and get JWT tokens
    /// </summary>
    [HttpPost("login")]
    [EnableRateLimiting("Auth")] // 5 req/min - Brute force protection
    [ProducesResponseType(typeof(LoginResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [SwaggerOperation(Summary = "Login", Description = "Login with email and password to get JWT tokens")]
    public async Task<ActionResult<LoginResponse>> Login([FromBody] LoginRequest request)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var user = await _userManager.FindByEmailAsync(request.Email);
        if (user == null)
            return Unauthorized(new { error = "Invalid email or password" });

        var result = await _signInManager.CheckPasswordSignInAsync(user, request.Password, lockoutOnFailure: true);

        if (!result.Succeeded)
        {
            if (result.IsLockedOut)
                return Unauthorized(new { error = "Account locked. Please try again later." });
            return Unauthorized(new { error = "Invalid email or password" });
        }

        var roles = await _userManager.GetRolesAsync(user);
    
    // Fetch customer to get customerId for token
    var customer = await _unitOfWork.Customers
        .Query()
        .FirstOrDefaultAsync(c => c.ApplicationUserId == user.Id);
    
    var accessToken = _jwtService.GenerateToken(user.Id, user.Email!, roles, customer?.CustomerId);
        
        var ipAddress = GetIpAddress();
        var userAgent = GetUserAgent();
        var refreshToken = await _jwtService.CreateRefreshTokenAsync(user.Id, ipAddress, userAgent, request.DeviceName);

        SetRefreshTokenCookie(refreshToken.Token, refreshToken.ExpiresAt);

        return Ok(new LoginResponse
        {
            AccessToken = accessToken,
            RefreshToken = refreshToken.Token,
            AccessTokenExpiration = DateTime.UtcNow.AddDays(7),
            RefreshTokenExpiration = refreshToken.ExpiresAt,
            Email = user.Email!,
            FirstName = user.FirstName,
            LastName = user.LastName
        });
    }

    /// <summary>
    /// Refresh access token using refresh token
    /// </summary>
    [HttpPost("refresh")]
    [EnableRateLimiting("Refresh")] // 10 req/min
    [ProducesResponseType(typeof(LoginResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [SwaggerOperation(Summary = "Refresh token", Description = "Get new access token using refresh token")]
    public async Task<ActionResult<LoginResponse>> RefreshToken([FromBody] RefreshTokenRequest? request)
    {
        var refreshToken = request?.RefreshToken ?? Request.Cookies["refreshToken"];

        if (string.IsNullOrEmpty(refreshToken))
            return Unauthorized(new { error = "Refresh token required" });

        var ipAddress = GetIpAddress();
        var userAgent = GetUserAgent();

        var isValid = await _jwtService.ValidateRefreshTokenAsync(refreshToken, ipAddress, userAgent);
        if (!isValid)
            return Unauthorized(new { error = "Invalid or expired refresh token" });

        var newRefreshToken = await _jwtService.RefreshTokenAsync(refreshToken, ipAddress, userAgent);

        var user = await _userManager.FindByIdAsync(newRefreshToken.UserId);
        var roles = await _userManager.GetRolesAsync(user!);
        var newAccessToken = _jwtService.GenerateToken(user!.Id, user.Email!, roles);

        SetRefreshTokenCookie(newRefreshToken.Token, newRefreshToken.ExpiresAt);

        return Ok(new LoginResponse
        {
            AccessToken = newAccessToken,
            RefreshToken = newRefreshToken.Token,
            AccessTokenExpiration = DateTime.UtcNow.AddDays(7),
            RefreshTokenExpiration = newRefreshToken.ExpiresAt,
            Email = user.Email!,
            FirstName = user.FirstName,
            LastName = user.LastName
        });
    }

    /// <summary>
    /// Revoke a refresh token
    /// </summary>
    [HttpPost("revoke")]
    [Authorize]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [SwaggerOperation(Summary = "Revoke token", Description = "Revoke a specific refresh token")]
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
    /// Revoke all refresh tokens for current user (logout from all devices)
    /// </summary>
    [HttpPost("revoke-all")]
    [Authorize]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [SwaggerOperation(Summary = "Revoke all tokens", Description = "Logout from all devices")]
    public async Task<IActionResult> RevokeAllTokens()
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        await _jwtService.RevokeAllUserTokensAsync(userId!);
        Response.Cookies.Delete("refreshToken");
        return Ok(new { message = "All tokens revoked successfully" });
    }

    /// <summary>
    /// Get current user information
    /// </summary>
    [HttpGet("me")]
    [Authorize]
    [ProducesResponseType(typeof(UserDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [SwaggerOperation(Summary = "Get current user", Description = "Get authenticated user information")]
    public async Task<ActionResult<UserDto>> GetCurrentUser()
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

        if (string.IsNullOrEmpty(userId))
            return Unauthorized();

        var user = await _userManager.FindByIdAsync(userId);
        if (user == null)
            return Unauthorized();

        var roles = await _userManager.GetRolesAsync(user);

        return Ok(new UserDto
        {
            UserId = user.Id,
            Email = user.Email!,
            FirstName = user.FirstName,
            LastName = user.LastName,
            Roles = roles.ToList()
        });
    }
}

// DTOs
public class LoginResponse
{
    public string AccessToken { get; set; } = string.Empty;
    public string RefreshToken { get; set; } = string.Empty;
    public DateTime AccessTokenExpiration { get; set; }
    public DateTime RefreshTokenExpiration { get; set; }
    public string Email { get; set; } = string.Empty;
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
}

public class RefreshTokenRequest
{
    public string? RefreshToken { get; set; }
}

public class RevokeTokenRequest
{
    public string? RefreshToken { get; set; }
}
