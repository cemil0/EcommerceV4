using ECommerce.Application.DTOs.Admin;
using ECommerce.Application.DTOs.Common;
using ECommerce.Application.Interfaces.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ECommerce.Api.Controllers.Admin;

[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/Admin/[controller]")]
[Authorize(Roles = "Admin")]
public class ProductsController : ControllerBase
{
    private readonly IAdminProductService _adminProductService;

    public ProductsController(IAdminProductService adminProductService)
    {
        _adminProductService = adminProductService;
    }

    [HttpGet]
    public async Task<ActionResult<PagedResponse<AdminProductDto>>> GetProducts([FromQuery] PagedRequest request)
    {
        var result = await _adminProductService.GetProductsAsync(request);
        return Ok(result);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<AdminProductDetailDto>> GetProduct(int id)
    {
        var result = await _adminProductService.GetProductDetailAsync(id);
        if (result == null)
            return NotFound($"Product {id} not found");

        return Ok(result);
    }

    [HttpPost]
    public async Task<ActionResult<AdminProductDto>> CreateProduct([FromBody] CreateProductRequest request)
    {
        try
        {
            var result = await _adminProductService.CreateProductAsync(request);
            return CreatedAtAction(nameof(GetProduct), new { id = result.ProductId }, result);
        }
        catch (Exception ex)
        {
            // Log the error (optional)
            Console.WriteLine($"Error creating product: {ex.Message}");
            if (ex.InnerException != null)
                Console.WriteLine($"Inner Exception: {ex.InnerException.Message}");

            return BadRequest($"Error creating product: {ex.Message} {(ex.InnerException != null ? "Inner: " + ex.InnerException.Message : "")}");
        }
    }

    [HttpPut("{id}")]
    public async Task<ActionResult<AdminProductDto>> UpdateProduct(int id, [FromBody] UpdateProductRequest request)
    {
        try
        {
            var result = await _adminProductService.UpdateProductAsync(id, request);
            if (result == null)
                return NotFound($"Product {id} not found");

            return Ok(result);
        }
        catch (Exception ex)
        {
            return BadRequest($"Error updating product: {ex.Message}");
        }
    }

    [HttpPut("{id}/activate")]
    public async Task<IActionResult> ActivateProduct(int id)
    {
        var success = await _adminProductService.ActivateProductAsync(id);
        if (!success)
            return NotFound($"Product {id} not found");

        return Ok("Product activated successfully");
    }

    [HttpPut("{id}/deactivate")]
    public async Task<IActionResult> DeactivateProduct(int id)
    {
        var success = await _adminProductService.DeactivateProductAsync(id);
        if (!success)
            return NotFound($"Product {id} not found");

        return Ok("Product deactivated successfully");
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteProduct(int id)
    {
        var success = await _adminProductService.DeleteProductAsync(id);
        if (!success)
            return NotFound($"Product {id} not found");

        return Ok("Product deleted successfully");
    }

    // Variant CRUD
    [HttpPost("{id}/variants")]
    public async Task<ActionResult<AdminVariantDto>> CreateVariant(int id, [FromBody] CreateVariantRequest request)
    {
        var result = await _adminProductService.CreateVariantAsync(id, request);
        if (result == null)
            return NotFound($"Product {id} not found");

        return CreatedAtAction(nameof(GetProduct), new { id }, result);
    }

    [HttpPut("{id}/variants/{variantId}")]
    public async Task<IActionResult> UpdateVariant(int id, int variantId, [FromBody] UpdateVariantRequest request)
    {
        var success = await _adminProductService.UpdateVariantAsync(id, variantId, request);
        if (!success)
            return NotFound($"Variant {variantId} not found for product {id}");

        return Ok("Variant updated successfully");
    }

    [HttpDelete("{id}/variants/{variantId}")]
    public async Task<IActionResult> DeleteVariant(int id, int variantId)
    {
        var success = await _adminProductService.DeleteVariantAsync(id, variantId);
        if (!success)
            return NotFound($"Variant {variantId} not found for product {id}");

        return Ok("Variant deleted successfully");
    }

    // Stock Management
    [HttpPut("{id}/variants/{variantId}/stock")]
    public async Task<IActionResult> UpdateStock(int id, int variantId, [FromBody] UpdateStockRequest request)
    {
        var success = await _adminProductService.UpdateStockAsync(id, variantId, request);
        if (!success)
            return NotFound($"Variant {variantId} not found for product {id}");

        return Ok("Stock updated successfully");
    }

    [HttpPost("bulk-stock-update")]
    public async Task<IActionResult> BulkUpdateStock([FromBody] BulkStockUpdateRequest request)
    {
        var success = await _adminProductService.BulkUpdateStockAsync(request);
        return Ok("Bulk stock update completed");
    }

    // Image Management
    [HttpPost("{id}/images")]
    public async Task<ActionResult<ProductImageDto>> UploadImage(int id, IFormFile file, [FromForm] UploadImageRequest? request)
    {
        if (file == null || file.Length == 0)
            return BadRequest("No file uploaded");

        // Validate file type
        var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".webp" };
        var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
        if (!allowedExtensions.Contains(extension))
            return BadRequest("Invalid file type. Only JPG, PNG, and WEBP are allowed.");

        // Validate file size (5MB max)
        if (file.Length > 5 * 1024 * 1024)
            return BadRequest("File size exceeds 5MB limit");

        try
        {
            using var stream = file.OpenReadStream();
            var uploadRequest = request ?? new UploadImageRequest();
            var result = await _adminProductService.UploadImageAsync(id, stream, file.FileName, uploadRequest);
            return CreatedAtAction(nameof(GetProductImages), new { id }, result);
        }
        catch (InvalidOperationException ex)
        {
            return NotFound(ex.Message);
        }
    }

    [HttpGet("{id}/images")]
    public async Task<ActionResult<IEnumerable<ProductImageDto>>> GetProductImages(int id)
    {
        var images = await _adminProductService.GetProductImagesAsync(id);
        return Ok(images);
    }

    [HttpDelete("{id}/images/{imageId}")]
    public async Task<IActionResult> DeleteImage(int id, int imageId)
    {
        var success = await _adminProductService.DeleteImageAsync(id, imageId);
        if (!success)
            return NotFound($"Image {imageId} not found for product {id}");

        return Ok("Image deleted successfully");
    }

    [HttpPut("{id}/images/{imageId}/set-primary")]
    public async Task<IActionResult> SetPrimaryImage(int id, int imageId)
    {
        var success = await _adminProductService.SetPrimaryImageAsync(id, imageId);
        if (!success)
            return NotFound($"Image {imageId} not found for product {id}");

        return Ok("Primary image set successfully");
    }

    [HttpPut("{id}/images/reorder")]
    public async Task<IActionResult> ReorderImages(int id, [FromBody] ReorderImagesRequest request)
    {
        var success = await _adminProductService.ReorderImagesAsync(id, request);
        return Ok("Images reordered successfully");
    }

    [HttpPost("{id}/images/bulk")]
    public async Task<ActionResult<IEnumerable<ProductImageDto>>> BulkUploadImages(int id, List<IFormFile> files)
    {
        if (files == null || files.Count == 0)
            return BadRequest("No files uploaded");

        if (files.Count > 20)
            return BadRequest("Maximum 20 files allowed per bulk upload");

        // Validate all files first
        var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".webp" };
        foreach (var file in files)
        {
            var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
            if (!allowedExtensions.Contains(extension))
                return BadRequest($"Invalid file type: {file.FileName}. Only JPG, PNG, and WEBP are allowed.");

            if (file.Length > 5 * 1024 * 1024)
                return BadRequest($"File {file.FileName} exceeds 5MB limit");
        }

        try
        {
            var fileStreams = files.Select(f => (f.OpenReadStream(), f.FileName));
            var results = await _adminProductService.BulkUploadImagesAsync(id, fileStreams);
            
            return Ok(new
            {
                Message = $"Successfully uploaded {results.Count()} images",
                Images = results
            });
        }
        catch (InvalidOperationException ex)
        {
            return NotFound(ex.Message);
        }
    }
}
