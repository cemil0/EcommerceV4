using ECommerce.Application.DTOs;
using ECommerce.Application.Interfaces.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;

namespace ECommerce.Api.Controllers;

[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/[controller]")]
public class CategoriesController : ControllerBase
{
    private readonly ICategoryService _categoryService;

    public CategoriesController(ICategoryService categoryService)
    {
        _categoryService = categoryService;
    }

    /// <summary>
    /// Get all root categories
    /// </summary>
    [HttpGet]
    [AllowAnonymous]
    [ProducesResponseType(typeof(IEnumerable<CategoryDto>), StatusCodes.Status200OK)]
    [SwaggerOperation(Summary = "Get all categories", Description = "Retrieve all root categories")]
    public async Task<ActionResult<IEnumerable<CategoryDto>>> GetCategories()
    {
        var categories = await _categoryService.GetRootCategoriesAsync();
        return Ok(categories);
    }

    /// <summary>
    /// Get category by ID
    /// </summary>
    [HttpGet("{id}")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(CategoryDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [SwaggerOperation(Summary = "Get category by ID", Description = "Retrieve a specific category by its ID")]
    public async Task<ActionResult<CategoryDto>> GetCategory(int id)
    {
        var category = await _categoryService.GetByIdAsync(id);
        
        if (category == null)
            return NotFound();
            
        return Ok(category);
    }

    /// <summary>
    /// Get category by slug
    /// </summary>
    [HttpGet("slug/{slug}")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(CategoryDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [SwaggerOperation(Summary = "Get category by slug", Description = "Retrieve a category by its URL slug")]
    public async Task<ActionResult<CategoryDto>> GetCategoryBySlug(string slug)
    {
        var category = await _categoryService.GetBySlugAsync(slug);
        
        if (category == null)
            return NotFound();
            
        return Ok(category);
    }

    /// <summary>
    /// Get subcategories of a category
    /// </summary>
    [HttpGet("{id}/subcategories")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(IEnumerable<CategoryDto>), StatusCodes.Status200OK)]
    [SwaggerOperation(Summary = "Get subcategories", Description = "Retrieve all subcategories of a parent category")]
    public async Task<ActionResult<IEnumerable<CategoryDto>>> GetSubCategories(int id)
    {
        var subcategories = await _categoryService.GetSubCategoriesAsync(id);
        return Ok(subcategories);
    }
}

