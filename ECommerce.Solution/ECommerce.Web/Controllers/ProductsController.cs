using ECommerce.Application.Interfaces.Services;
using Microsoft.AspNetCore.Mvc;

namespace ECommerce.Web.Controllers;

public class ProductsController : Controller
{
    private readonly IProductService _productService;
    private readonly ICategoryService _categoryService;

    public ProductsController(IProductService productService, ICategoryService categoryService)
    {
        _productService = productService;
        _categoryService = categoryService;
    }

    // GET: /Products
    public async Task<IActionResult> Index(int? categoryId, string? search)
    {
        IEnumerable<Application.DTOs.ProductDto> products;

        if (!string.IsNullOrEmpty(search))
        {
            products = await _productService.SearchAsync(search);
            ViewBag.SearchTerm = search;
        }
        else if (categoryId.HasValue)
        {
            products = await _productService.GetByCategoryAsync(categoryId.Value);
            var category = await _categoryService.GetByIdAsync(categoryId.Value);
            ViewBag.CategoryName = category?.CategoryName;
        }
        else
        {
            products = await _productService.GetAllAsync();
        }

        // Get categories for sidebar
        ViewBag.Categories = await _categoryService.GetRootCategoriesAsync();

        return View(products);
    }

    // GET: /Products/Details/5
    public async Task<IActionResult> Details(int id)
    {
        var product = await _productService.GetByIdAsync(id);

        if (product == null)
        {
            return NotFound();
        }

        return View(product);
    }

    // GET: /Products/{slug}
    [HttpGet("Products/{slug}")]
    public async Task<IActionResult> DetailsBySlug(string slug)
    {
        var product = await _productService.GetBySlugAsync(slug);

        if (product == null)
        {
            return NotFound();
        }

        return View("Details", product);
    }
}
