using ECommerce.Application.DTOs.Admin;
using ECommerce.Application.Interfaces;
using ECommerce.Application.Interfaces.Services;
using ECommerce.Domain.Entities;
using System.Diagnostics;

namespace ECommerce.Infrastructure.Services;

public class BulkProductImportService : IBulkProductImportService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IEnumerable<IFileParser> _parsers;
    private readonly IFileStorageService _fileStorageService;
    private readonly HttpClient _httpClient;

    public BulkProductImportService(
        IUnitOfWork unitOfWork, 
        IEnumerable<IFileParser> parsers,
        IFileStorageService fileStorageService)
    {
        _unitOfWork = unitOfWork;
        _parsers = parsers;
        _fileStorageService = fileStorageService;
        _httpClient = new HttpClient();
        _httpClient.Timeout = TimeSpan.FromSeconds(30);
    }

    public async Task<BulkImportResultDto> ImportAsync(Stream fileStream, string fileName, BulkImportOptionsDto options)
    {
        var stopwatch = Stopwatch.StartNew();
        var result = new BulkImportResultDto();

        try
        {
            // Step 1: Select appropriate parser
            var parser = _parsers.FirstOrDefault(p => p.CanParse(fileName));
            if (parser == null)
            {
                result.Errors.Add(new BulkImportErrorDto
                {
                    RowNumber = 0,
                    Message = "Unsupported file format. Only .xlsx and .csv files are supported.",
                    Severity = "Error"
                });
                return result;
            }

            // Step 2: Parse file
            var rows = await parser.ParseAsync(fileStream, fileName);
            result.TotalRows = rows.Count;

            if (rows.Count == 0)
            {
                result.Errors.Add(new BulkImportErrorDto
                {
                    RowNumber = 0,
                    Message = "No data rows found in file",
                    Severity = "Error"
                });
                return result;
            }

            // Step 2: Validate all rows first
            var validRows = new List<(int rowNumber, BulkProductImportRowDto row)>();
            for (int i = 0; i < rows.Count; i++)
            {
                var rowNumber = i + 2; // Excel row (1-based + header)
                var row = rows[i];
                var errors = await ValidateRowAsync(row, rowNumber);

                if (errors.Any())
                {
                    result.Errors.AddRange(errors);
                    result.FailedCount++;
                }
                else
                {
                    validRows.Add((rowNumber, row));
                }
            }

            // Step 3: If strict mode and any errors, abort
            if (options.StrictMode && result.Errors.Any())
            {
                result.Errors.Insert(0, new BulkImportErrorDto
                {
                    RowNumber = 0,
                    Message = $"Strict mode: Import aborted due to {result.Errors.Count} validation errors",
                    Severity = "Error"
                });
                stopwatch.Stop();
                result.ProcessingTimeMs = stopwatch.ElapsedMilliseconds;
                return result;
            }

            // Step 4: Dry run check
            if (options.DryRun)
            {
                result.SuccessCount = validRows.Count;
                result.Warnings.Add(new BulkImportWarningDto
                {
                    RowNumber = 0,
                    Message = "Dry run mode: No data was saved"
                });
                stopwatch.Stop();
                result.ProcessingTimeMs = stopwatch.ElapsedMilliseconds;
                return result;
            }

            // Step 5: Process valid rows in transaction
            await _unitOfWork.BeginTransactionAsync();

            try
            {
                foreach (var (rowNumber, row) in validRows)
                {
                    try
                    {
                        await ProcessRowAsync(row, rowNumber, result);
                        result.SuccessCount++;
                    }
                    catch (Exception ex)
                    {
                        result.Errors.Add(new BulkImportErrorDto
                        {
                            RowNumber = rowNumber,
                            SKU = row.VariantSKU,
                            Message = $"Processing error: {ex.Message}",
                            Severity = "Error"
                        });
                        result.FailedCount++;

                        if (options.StrictMode)
                            throw; // Rollback entire transaction
                    }
                }

                await _unitOfWork.CommitTransactionAsync();
            }
            catch
            {
                await _unitOfWork.RollbackTransactionAsync();
                throw;
            }
        }
        catch (Exception ex)
        {
            result.Errors.Insert(0, new BulkImportErrorDto
            {
                RowNumber = 0,
                Message = $"Critical error: {ex.Message}",
                Severity = "Critical"
            });
        }

        stopwatch.Stop();
        result.ProcessingTimeMs = stopwatch.ElapsedMilliseconds;
        return result;
    }

    private async Task<List<BulkImportErrorDto>> ValidateRowAsync(BulkProductImportRowDto row, int rowNumber)
    {
        var errors = new List<BulkImportErrorDto>();

        // Required fields
        if (string.IsNullOrWhiteSpace(row.ProductName))
            errors.Add(new BulkImportErrorDto { RowNumber = rowNumber, Field = "ProductName", Message = "ProductName is required" });

        if (string.IsNullOrWhiteSpace(row.CategorySlug))
            errors.Add(new BulkImportErrorDto { RowNumber = rowNumber, Field = "CategorySlug", Message = "CategorySlug is required" });

        if (string.IsNullOrWhiteSpace(row.VariantSKU))
            errors.Add(new BulkImportErrorDto { RowNumber = rowNumber, Field = "VariantSKU", Message = "VariantSKU is required" });

        // Price validation
        if (row.BasePrice < 0)
            errors.Add(new BulkImportErrorDto { RowNumber = rowNumber, Field = "BasePrice", Message = "BasePrice cannot be negative" });

        if (row.CostPrice.HasValue && row.CostPrice < 0)
            errors.Add(new BulkImportErrorDto { RowNumber = rowNumber, Field = "CostPrice", Message = "CostPrice cannot be negative" });

        // Stock validation
        if (row.StockQuantity < 0)
            errors.Add(new BulkImportErrorDto { RowNumber = rowNumber, Field = "StockQuantity", Message = "StockQuantity cannot be negative" });

        // Category exists check - search by slug OR name (to allow spaces in Excel)
        var categoryInput = row.CategorySlug?.Trim();
        var category = (await _unitOfWork.Categories.FindAsync(c => 
            c.CategorySlug == categoryInput || 
            c.CategoryName == categoryInput ||
            c.CategorySlug == categoryInput.ToLower().Replace(" ", "-")
        )).FirstOrDefault();
        if (category == null)
            errors.Add(new BulkImportErrorDto { RowNumber = rowNumber, Field = "CategorySlug", Message = $"Category '{row.CategorySlug}' not found" });

        // SKU uniqueness check
        var existingVariant = (await _unitOfWork.ProductVariants.FindAsync(v => v.VariantSKU == row.VariantSKU)).FirstOrDefault();
        if (existingVariant != null)
            errors.Add(new BulkImportErrorDto { RowNumber = rowNumber, Field = "VariantSKU", Message = $"SKU '{row.VariantSKU}' already exists", Severity = "Warning" });

        return errors;
    }

    private async Task ProcessRowAsync(BulkProductImportRowDto row, int rowNumber, BulkImportResultDto result)
    {
        // Find category by slug OR name (to allow spaces in Excel)
        var categoryInput = row.CategorySlug?.Trim();
        var category = (await _unitOfWork.Categories.FindAsync(c => 
            c.CategorySlug == categoryInput || 
            c.CategoryName == categoryInput ||
            c.CategorySlug == categoryInput.ToLower().Replace(" ", "-")
        )).FirstOrDefault();
        
        if (category == null)
        {
            result.Errors.Add(new BulkImportErrorDto
            {
                RowNumber = rowNumber,
                SKU = row.VariantSKU,
                Message = $"Processing failed: Category '{row.CategorySlug}' not found.",
                Severity = "Error"
            });
            result.FailedCount++;
            return;
        }

        var product = (await _unitOfWork.Products.FindAsync(p => 
            p.ProductName == row.ProductName && 
            p.CategoryId == category.CategoryId)).FirstOrDefault();

        if (product == null)
        {
            // Create new product
            product = new Product
            {
                ProductName = row.ProductName,
                ProductSlug = GenerateSlug(row.ProductName),
                CategoryId = category.CategoryId,
                Brand = row.Brand,
                Manufacturer = row.Manufacturer,
                Model = row.Model,
                ShortDescription = row.ShortDescription,
                LongDescription = row.Description,
                IsFeatured = row.IsFeatured,
                IsNewArrival = row.IsNewArrival,
                IsActive = row.IsActive,
                IsVariantProduct = true,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
                SKU = row.VariantSKU.Split('-')[0] // Base SKU from variant
            };

            await _unitOfWork.Products.AddAsync(product);
            await _unitOfWork.SaveChangesAsync(); // Get ProductId

            result.Successes.Add(new BulkImportSuccessDto
            {
                RowNumber = rowNumber,
                SKU = row.VariantSKU,
                Message = $"Created new product: {row.ProductName}",
                Action = "Created"
            });
        }

        // Check if variant already exists
        var existingVariant = (await _unitOfWork.ProductVariants.FindAsync(v => v.VariantSKU == row.VariantSKU)).FirstOrDefault();
        
        if (existingVariant != null)
        {
            // Update existing variant
            existingVariant.BasePrice = row.BasePrice;
            existingVariant.CostPrice = row.CostPrice;
            existingVariant.StockQuantity = row.StockQuantity;
            existingVariant.IsActive = row.IsActive;
            existingVariant.UpdatedAt = DateTime.UtcNow;

            _unitOfWork.ProductVariants.Update(existingVariant);

            result.Warnings.Add(new BulkImportWarningDto
            {
                RowNumber = rowNumber,
                SKU = row.VariantSKU,
                Message = "Updated existing variant"
            });
        }
        else
        {
            // Create new variant
            var variant = new ProductVariant
            {
                ProductId = product.ProductId,
                VariantSKU = row.VariantSKU,
                VariantName = GenerateVariantName(row),
                Color = row.Color,
                Size = row.Size,
                RAM = row.RAM,
                Storage = row.Storage,
                BasePrice = row.BasePrice,
                CostPrice = row.CostPrice,
                StockQuantity = row.StockQuantity,
                ReservedQuantity = 0,
                IsActive = row.IsActive,
                IsDefault = false,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            await _unitOfWork.ProductVariants.AddAsync(variant);
        }

        await _unitOfWork.SaveChangesAsync();

        // Process images if provided
        if (!string.IsNullOrWhiteSpace(row.ImageUrls))
        {
            await ProcessImagesAsync(product.ProductId, row.ImageUrls, row.PrimaryImage, rowNumber, result);
        }
    }

    private async Task ProcessImagesAsync(int productId, string imageUrls, bool setPrimary, int rowNumber, BulkImportResultDto result)
    {
        var urls = imageUrls.Split(',', StringSplitOptions.RemoveEmptyEntries)
            .Select(u => u.Trim())
            .Where(u => !string.IsNullOrEmpty(u))
            .ToList();

        if (urls.Count > 20)
        {
            result.Warnings.Add(new BulkImportWarningDto
            {
                RowNumber = rowNumber,
                Message = $"Too many images ({urls.Count}). Only first 20 will be processed."
            });
            urls = urls.Take(20).ToList();
        }

        var existingImages = await _unitOfWork.ProductImages.GetByProductIdAsync(productId);
        var displayOrder = existingImages.Count();
        var isFirstImage = !existingImages.Any();

        // Use SemaphoreSlim for concurrent downloads (max 5 at a time)
        var semaphore = new SemaphoreSlim(5, 5);
        var downloadTasks = new List<Task<(bool success, ProductImage? image, string? error)>>();

        for (int i = 0; i < urls.Count; i++)
        {
            var index = i;
            var imageUrl = urls[i];
            var currentDisplayOrder = displayOrder + i;
            var isPrimary = (isFirstImage && i == 0) || (setPrimary && i == 0);

            var task = Task.Run(async () =>
            {
                await semaphore.WaitAsync();
                try
                {
                    // Check if URL or local file
                    if (imageUrl.StartsWith("http://", StringComparison.OrdinalIgnoreCase) ||
                        imageUrl.StartsWith("https://", StringComparison.OrdinalIgnoreCase))
                    {
                        // Download from URL
                        var savedImageUrl = await DownloadAndSaveImageAsync(imageUrl, productId);

                        // Create ProductImage entity
                        var productImage = new ProductImage
                        {
                            ProductId = productId,
                            ImageUrl = savedImageUrl,
                            IsPrimary = isPrimary,
                            DisplayOrder = currentDisplayOrder,
                            CreatedAt = DateTime.UtcNow
                        };

                        return (true, productImage, (string?)null);
                    }
                    else
                    {
                        // Local file path (not supported yet)
                        return (false, (ProductImage?)null, $"Local file paths not supported yet: {imageUrl}");
                    }
                }
                catch (Exception ex)
                {
                    return (false, (ProductImage?)null, $"Failed to process image {imageUrl}: {ex.Message}");
                }
                finally
                {
                    semaphore.Release();
                }
            });

            downloadTasks.Add(task);
        }

        // Wait for all downloads to complete
        var results = await Task.WhenAll(downloadTasks);

        // Process results
        foreach (var (success, image, error) in results)
        {
            if (success && image != null)
            {
                // If setting as primary, remove primary from others
                if (image.IsPrimary)
                {
                    foreach (var img in existingImages.Where(img => img.IsPrimary))
                    {
                        img.IsPrimary = false;
                        _unitOfWork.ProductImages.Update(img);
                    }
                }

                await _unitOfWork.ProductImages.AddAsync(image);
            }
            else if (error != null)
            {
                result.Warnings.Add(new BulkImportWarningDto
                {
                    RowNumber = rowNumber,
                    Message = error
                });
            }
        }

        await _unitOfWork.SaveChangesAsync();
    }

    private async Task<string> DownloadAndSaveImageAsync(string imageUrl, int productId)
    {
        // Download image
        var response = await _httpClient.GetAsync(imageUrl);
        response.EnsureSuccessStatusCode();

        using var imageStream = await response.Content.ReadAsStreamAsync();
        var fileName = Path.GetFileName(new Uri(imageUrl).LocalPath);
        
        if (string.IsNullOrEmpty(fileName))
            fileName = $"{Guid.NewGuid()}.jpg";

        // Save with WebP conversion and thumbnail
        return await _fileStorageService.SaveFileAsync(imageStream, fileName, $"products/{productId}");
    }

    private string GenerateSlug(string name)
    {
        return name.ToLowerInvariant()
            .Replace(" ", "-")
            .Replace("ı", "i")
            .Replace("ğ", "g")
            .Replace("ü", "u")
            .Replace("ş", "s")
            .Replace("ö", "o")
            .Replace("ç", "c");
    }

    private string GenerateVariantName(BulkProductImportRowDto row)
    {
        var parts = new List<string>();
        
        if (!string.IsNullOrEmpty(row.Color)) parts.Add(row.Color);
        if (!string.IsNullOrEmpty(row.Size)) parts.Add(row.Size);
        if (!string.IsNullOrEmpty(row.Storage)) parts.Add(row.Storage);
        if (!string.IsNullOrEmpty(row.RAM)) parts.Add(row.RAM);

        return parts.Any() ? string.Join(" / ", parts) : "Default";
    }
}
