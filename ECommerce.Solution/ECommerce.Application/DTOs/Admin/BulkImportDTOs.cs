namespace ECommerce.Application.DTOs.Admin;

public class BulkProductImportRowDto
{
    // Product Info
    public string ProductName { get; set; } = string.Empty;
    public string CategorySlug { get; set; } = string.Empty;
    public string? Brand { get; set; }
    public string? Manufacturer { get; set; }
    public string? Model { get; set; }
    public string? ShortDescription { get; set; }
    public string? Description { get; set; }
    public bool IsFeatured { get; set; }
    public bool IsNewArrival { get; set; }
    
    // Variant Info
    public string VariantSKU { get; set; } = string.Empty;
    public string? Color { get; set; }
    public string? Size { get; set; }
    public string? RAM { get; set; }
    public string? Storage { get; set; }
    
    // Pricing
    public decimal BasePrice { get; set; }
    public decimal? CostPrice { get; set; }
    
    // Stock
    public int StockQuantity { get; set; }
    
    // Images
    public string? ImageUrls { get; set; } // Comma-separated
    public bool PrimaryImage { get; set; }
    
    // Status
    public bool IsActive { get; set; } = true;
}

public class BulkImportResultDto
{
    public int TotalRows { get; set; }
    public int SuccessCount { get; set; }
    public int FailedCount { get; set; }
    public long ProcessingTimeMs { get; set; }
    public List<BulkImportErrorDto> Errors { get; set; } = new();
    public List<BulkImportWarningDto> Warnings { get; set; } = new();
    public List<BulkImportSuccessDto> Successes { get; set; } = new();
}

public class BulkImportErrorDto
{
    public int RowNumber { get; set; }
    public string? SKU { get; set; }
    public string? Field { get; set; }
    public string Message { get; set; } = string.Empty;
    public string Severity { get; set; } = "Error";
}

public class BulkImportSuccessDto
{
    public int RowNumber { get; set; }
    public string? SKU { get; set; }
    public string Message { get; set; } = string.Empty;
    public string Action { get; set; } = "Created"; // Created, Updated
}

public class BulkImportWarningDto
{
    public int RowNumber { get; set; }
    public string? SKU { get; set; }
    public string Message { get; set; } = string.Empty;
}

public class BulkImportOptionsDto
{
    public bool StrictMode { get; set; } = false;
    public bool DryRun { get; set; } = false;
    public int MaxConcurrentImages { get; set; } = 5;
}
