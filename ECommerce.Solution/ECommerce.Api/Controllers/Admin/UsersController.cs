using ECommerce.Application.DTOs.Admin;
using ECommerce.Application.DTOs.Common;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ECommerce.Api.Controllers.Admin;

[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/Admin/[controller]")]
[Authorize(Roles = "Admin")]
public class UsersController : ControllerBase
{
    private readonly UserManager<ECommerce.Domain.Entities.ApplicationUser> _userManager;
    private readonly RoleManager<IdentityRole> _roleManager;

    public UsersController(
        UserManager<ECommerce.Domain.Entities.ApplicationUser> userManager,
        RoleManager<IdentityRole> roleManager)
    {
        _userManager = userManager;
        _roleManager = roleManager;
    }

    [HttpGet]
    public async Task<ActionResult<PagedResponse<AdminUserDto>>> GetUsers([FromQuery] PagedRequest request)
    {
        var query = _userManager.Users.AsQueryable();

        // Get total count
        var totalCount = await query.CountAsync();

        // Apply pagination
        var users = await query
            .OrderByDescending(u => u.CreatedAt)
            .Skip((request.Page - 1) * request.PageSize)
            .Take(request.PageSize)
            .ToListAsync();

        // Map to DTOs with roles
        var userDtos = new List<AdminUserDto>();
        foreach (var user in users)
        {
            var roles = await _userManager.GetRolesAsync(user);
            userDtos.Add(new AdminUserDto
            {
                Id = user.Id,
                Email = user.Email ?? string.Empty,
                FullName = $"{user.FirstName} {user.LastName}",
                Roles = roles.ToList(),
                IsActive = user.IsActive,
                CreatedAt = user.CreatedAt,
                LastLogin = user.LastLoginAt
            });
        }

        return Ok(new PagedResponse<AdminUserDto>
        {
            Data = userDtos,
            TotalCount = totalCount,
            Page = request.Page,
            PageSize = request.PageSize
        });
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<AdminUserDto>> GetUser(string id)
    {
        var user = await _userManager.FindByIdAsync(id);
        if (user == null)
            return NotFound($"User with ID {id} not found");

        var roles = await _userManager.GetRolesAsync(user);

        return Ok(new AdminUserDto
        {
            Id = user.Id,
            Email = user.Email ?? string.Empty,
            FullName = $"{user.FirstName} {user.LastName}",
            Roles = roles.ToList(),
            IsActive = user.IsActive,
            CreatedAt = user.CreatedAt,
            LastLogin = user.LastLoginAt
        });
    }

    [HttpPost]
    public async Task<ActionResult<AdminUserDto>> CreateUser([FromBody] CreateUserRequest request)
    {
        // Check if user exists
        var existingUser = await _userManager.FindByEmailAsync(request.Email);
        if (existingUser != null)
            return BadRequest("User with this email already exists");

        // Create user
        var user = new ECommerce.Domain.Entities.ApplicationUser
        {
            UserName = request.Email,
            Email = request.Email,
            FirstName = request.FirstName,
            LastName = request.LastName,
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };

        var result = await _userManager.CreateAsync(user, request.Password);
        if (!result.Succeeded)
            return BadRequest(result.Errors);

        // Assign roles
        if (request.Roles.Any())
        {
            foreach (var role in request.Roles)
            {
                if (await _roleManager.RoleExistsAsync(role))
                {
                    await _userManager.AddToRoleAsync(user, role);
                }
            }
        }

        var roles = await _userManager.GetRolesAsync(user);

        return CreatedAtAction(nameof(GetUser), new { id = user.Id }, new AdminUserDto
        {
            Id = user.Id,
            Email = user.Email,
            FullName = $"{user.FirstName} {user.LastName}",
            Roles = roles.ToList(),
            IsActive = user.IsActive,
            CreatedAt = user.CreatedAt
        });
    }

    [HttpPut("{id}")]
    public async Task<ActionResult<AdminUserDto>> UpdateUser(string id, [FromBody] UpdateUserRequest request)
    {
        var user = await _userManager.FindByIdAsync(id);
        if (user == null)
            return NotFound($"User with ID {id} not found");

        user.FirstName = request.FirstName;
        user.LastName = request.LastName;
        user.IsActive = request.IsActive;
        user.UpdatedAt = DateTime.UtcNow;

        var result = await _userManager.UpdateAsync(user);
        if (!result.Succeeded)
            return BadRequest(result.Errors);

        var roles = await _userManager.GetRolesAsync(user);

        return Ok(new AdminUserDto
        {
            Id = user.Id,
            Email = user.Email ?? string.Empty,
            FullName = $"{user.FirstName} {user.LastName}",
            Roles = roles.ToList(),
            IsActive = user.IsActive,
            CreatedAt = user.CreatedAt,
            LastLogin = user.LastLoginAt
        });
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteUser(string id)
    {
        var user = await _userManager.FindByIdAsync(id);
        if (user == null)
            return NotFound($"User with ID {id} not found");

        // Soft delete
        user.IsActive = false;
        user.UpdatedAt = DateTime.UtcNow;
        await _userManager.UpdateAsync(user);

        return NoContent();
    }

    [HttpGet("{id}/roles")]
    public async Task<ActionResult<List<string>>> GetUserRoles(string id)
    {
        var user = await _userManager.FindByIdAsync(id);
        if (user == null)
            return NotFound($"User with ID {id} not found");

        var roles = await _userManager.GetRolesAsync(user);
        return Ok(roles.ToList());
    }

    [HttpPost("{id}/roles")]
    public async Task<IActionResult> AssignRole(string id, [FromBody] AssignRoleRequest request)
    {
        var user = await _userManager.FindByIdAsync(id);
        if (user == null)
            return NotFound($"User with ID {id} not found");

        if (!await _roleManager.RoleExistsAsync(request.Role))
            return BadRequest($"Role '{request.Role}' does not exist");

        if (await _userManager.IsInRoleAsync(user, request.Role))
            return BadRequest($"User already has role '{request.Role}'");

        var result = await _userManager.AddToRoleAsync(user, request.Role);
        if (!result.Succeeded)
            return BadRequest(result.Errors);

        return Ok($"Role '{request.Role}' assigned successfully");
    }

    [HttpDelete("{id}/roles/{role}")]
    public async Task<IActionResult> RemoveRole(string id, string role)
    {
        var user = await _userManager.FindByIdAsync(id);
        if (user == null)
            return NotFound($"User with ID {id} not found");

        if (!await _userManager.IsInRoleAsync(user, role))
            return BadRequest($"User does not have role '{role}'");

        var result = await _userManager.RemoveFromRoleAsync(user, role);
        if (!result.Succeeded)
            return BadRequest(result.Errors);

        return Ok($"Role '{role}' removed successfully");
    }
}
