using ECommerce.Application.DTOs;
using ECommerce.Application.DTOs.Common;
using ECommerce.Application.Interfaces.Services;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;

namespace ECommerce.Api.Controllers;

[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/[controller]")]
public class ProductsController : ControllerBase
{
    private readonly IProductService _productService;

    public ProductsController(IProductService productService)
    {
        _productService = productService;
    }

    /// <summary>
    /// Get paginated list of products
    /// </summary>
    /// <param name="request">Pagination parameters</param>
    /// <returns>Paginated list of products</returns>
    /// <response code="200">Returns paginated products</response>
    [HttpGet]
    [ProducesResponseType(typeof(PagedResponse<ProductDto>), StatusCodes.Status200OK)]
    [SwaggerOperation(
        Summary = "Get paginated products",
        Description = "Get a paginated list of active products with optional sorting")]
    public async Task<ActionResult<PagedResponse<ProductDto>>> GetProducts([FromQuery] PagedRequest request)
    {
        var products = await _productService.GetPagedAsync(request);
        return Ok(products);
    }

    /// <summary>
    /// Get product by ID
    /// </summary>
    /// <param name="id">Product ID</param>
    /// <returns>Product details</returns>
    /// <response code="200">Returns the product</response>
    /// <response code="404">Product not found</response>
    [HttpGet("{id}")]
    [ProducesResponseType(typeof(ProductDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [SwaggerOperation(Summary = "Get product by ID", Description = "Get detailed product information by ID")]
    public async Task<ActionResult<ProductDto>> GetProduct(int id)
    {
        var product = await _productService.GetByIdAsync(id);
        
        if (product == null)
            return NotFound();
        
        return Ok(product);
    }

    /// <summary>
    /// Get product by slug
    /// </summary>
    /// <param name="slug">Product slug</param>
    /// <returns>Product details</returns>
    /// <response code="200">Returns the product</response>
    /// <response code="404">Product not found</response>
    [HttpGet("slug/{slug}")]
    [ProducesResponseType(typeof(ProductDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [SwaggerOperation(Summary = "Get product by slug", Description = "Get product by URL-friendly slug")]
    public async Task<ActionResult<ProductDto>> GetProductBySlug(string slug)
    {
        var product = await _productService.GetBySlugAsync(slug);
        
        if (product == null)
            return NotFound();
        
        return Ok(product);
    }

    /// <summary>
    /// Get paginated products by category
    /// </summary>
    /// <param name="categoryId">Category ID</param>
    /// <param name="request">Pagination parameters</param>
    /// <returns>Paginated list of products in category</returns>
    /// <response code="200">Returns paginated products</response>
    [HttpGet("category/{categoryId}")]
    [ProducesResponseType(typeof(PagedResponse<ProductDto>), StatusCodes.Status200OK)]
    [SwaggerOperation(
        Summary = "Get products by category",
        Description = "Get a paginated list of products filtered by category")]
    public async Task<ActionResult<PagedResponse<ProductDto>>> GetProductsByCategory(
        int categoryId,
        [FromQuery] PagedRequest request)
    {
        var products = await _productService.GetPagedByCategoryAsync(categoryId, request);
        return Ok(products);
    }

    /// <summary>
    /// Search products with pagination
    /// </summary>
    /// <param name="searchTerm">Search term</param>
    /// <param name="request">Pagination parameters</param>
    /// <returns>Paginated search results</returns>
    /// <response code="200">Returns paginated search results</response>
    [HttpGet("search")]
    [ProducesResponseType(typeof(PagedResponse<ProductDto>), StatusCodes.Status200OK)]
    [SwaggerOperation(
        Summary = "Search products",
        Description = "Search products by name, brand, or SKU with pagination")]
    public async Task<ActionResult<PagedResponse<ProductDto>>> SearchProducts(
        [FromQuery] string searchTerm,
        [FromQuery] PagedRequest request)
    {
        var products = await _productService.SearchPagedAsync(searchTerm, request);
        return Ok(products);
    }

    /// <summary>
    /// Get featured products
    /// </summary>
    /// <param name="count">Number of products to return</param>
    /// <returns>List of featured products</returns>
    /// <response code="200">Returns featured products</response>
    [HttpGet("featured")]
    [ProducesResponseType(typeof(IEnumerable<ProductDto>), StatusCodes.Status200OK)]
    [SwaggerOperation(
        Summary = "Get featured products",
        Description = "Get a list of featured products (not paginated)")]
    public async Task<ActionResult<IEnumerable<ProductDto>>> GetFeaturedProducts([FromQuery] int count = 10)
    {
        var products = await _productService.GetFeaturedProductsAsync(count);
        return Ok(products);
    }
}
