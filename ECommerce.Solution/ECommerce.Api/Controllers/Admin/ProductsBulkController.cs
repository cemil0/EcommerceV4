using ECommerce.Application.DTOs.Admin;
using ECommerce.Application.Interfaces.Services;
using ECommerce.Api.BackgroundServices;
using ECommerce.Domain.Entities;
using ECommerce.Infrastructure.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ECommerce.Api.Controllers.Admin;

[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/Admin/Products")]
[Authorize(Roles = "Admin")]
public class ProductsBulkController : ControllerBase
{
    private readonly IBulkProductImportService _bulkImportService;

    public ProductsBulkController(IBulkProductImportService bulkImportService)
    {
        _bulkImportService = bulkImportService;
    }

    /// <summary>
    /// Bulk import products from Excel file
    /// </summary>
    /// <param name="file">Excel file (.xlsx)</param>
    /// <param name="strictMode">If true, abort on any error</param>
    /// <param name="dryRun">If true, validate only without saving</param>
    [HttpPost("bulk-upload")]
    public async Task<ActionResult<BulkImportResultDto>> BulkUpload(
        IFormFile file,
        [FromForm] bool strictMode = false,
        [FromForm] bool dryRun = false)
    {
        if (file == null || file.Length == 0)
            return BadRequest("No file uploaded");

        var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
        if (extension != ".xlsx" && extension != ".csv")
            return BadRequest("Only Excel (.xlsx) and CSV (.csv) files are supported");

        // Max 50MB file size
        if (file.Length > 50 * 1024 * 1024)
            return BadRequest("File size exceeds 50MB limit");

        try
        {
            using var stream = file.OpenReadStream();
            var options = new BulkImportOptionsDto
            {
                StrictMode = strictMode,
                DryRun = dryRun
            };

            var result = await _bulkImportService.ImportAsync(stream, file.FileName, options);

            if (result.Errors.Any(e => e.Severity == "Critical"))
                return StatusCode(500, result);

            return Ok(result);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[CRITICAL ERROR] BulkUpload failed: {ex}");
            if (ex.InnerException != null)
            {
                Console.WriteLine($"[CRITICAL ERROR] Inner Exception: {ex.InnerException}");
            }

            return StatusCode(500, new BulkImportResultDto
            {
                Errors = new List<BulkImportErrorDto>
                {
                    new BulkImportErrorDto
                    {
                        RowNumber = 0,
                        Message = $"Upload failed: {ex.Message}",
                        Severity = "Critical"
                    }
                }
            });
        }
    }

    /// <summary>
    /// Async bulk import (returns JobId)
    /// </summary>
    [HttpPost("bulk-upload-async")]
    public async Task<ActionResult<Guid>> BulkUploadAsync(
        IFormFile file,
        [FromServices] BulkImportBackgroundService backgroundService,
        [FromServices] ECommerceDbContext dbContext) // Helper for job creation
    {
        if (file == null || file.Length == 0)
            return BadRequest("No file uploaded");

        var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
        if (extension != ".xlsx" && extension != ".csv")
            return BadRequest("Only Excel (.xlsx) and CSV (.csv) files are supported");

        // Save file to temp
        var fileName = $"{Guid.NewGuid()}{extension}";
        var tempPath = Path.Combine(Path.GetTempPath(), fileName);
        
        using (var stream = new FileStream(tempPath, FileMode.Create))
        {
            await file.CopyToAsync(stream);
        }

        // Create Job Record
        var job = new BulkImportJob
        {
            JobId = Guid.NewGuid(),
            FileName = fileName,
            Status = "Pending",
            UserId = Guid.Empty, // Should get from User.Identity
            CreatedAt = DateTime.UtcNow
        };

        dbContext.BulkImportJobs.Add(job);
        await dbContext.SaveChangesAsync();

        // Queue Job
        await backgroundService.QueueJobAsync(job.JobId);

        return Accepted(new { JobId = job.JobId, StatusUrl = $"/api/v1/Admin/Products/bulk-jobs/{job.JobId}" });
    }

    /// <summary>
    /// Get import job status
    /// </summary>
    [HttpGet("bulk-jobs/{jobId}")]
    public async Task<ActionResult<BulkImportJob>> GetJobStatus(
        Guid jobId,
        [FromServices] ECommerceDbContext dbContext)
    {
        var job = await dbContext.BulkImportJobs.FindAsync(jobId);
        if (job == null)
            return NotFound("Job not found");

        return Ok(job);
    }

    /// <summary>
    /// Download import job report (errors/warnings)
    /// </summary>
    [HttpGet("bulk-jobs/{jobId}/report")]
    public async Task<IActionResult> GetJobReport(
        Guid jobId,
        [FromQuery] string format = "xlsx",
        [FromServices] ECommerceDbContext dbContext = null)
    {
        var job = await dbContext.BulkImportJobs.FindAsync(jobId);
        if (job == null)
            return NotFound("Job not found");

        if (string.IsNullOrEmpty(job.ResultJson))
            return BadRequest("Job has no results yet");

        var result = System.Text.Json.JsonSerializer.Deserialize<BulkImportResultDto>(job.ResultJson);
        
        // If no errors or warnings, nothing to report
        if (!result.Errors.Any() && !result.Warnings.Any())
            return Ok("No errors or warnings to report");

        if (format.ToLower() == "csv")
        {
            var csv = new System.Text.StringBuilder();
            csv.AppendLine("Type,RowNumber,SKU,Field,Message");
            
            foreach (var error in result.Errors)
                csv.AppendLine($"Error,{error.RowNumber},{error.SKU},{error.Field},\"{error.Message.Replace("\"", "\"\"")}\"");
                
            foreach (var warning in result.Warnings)
                csv.AppendLine($"Warning,{warning.RowNumber},{warning.SKU},,\"{warning.Message.Replace("\"", "\"\"")}\"");

            var bytes = System.Text.Encoding.UTF8.GetBytes(csv.ToString());
            return File(bytes, "text/csv", $"import-report-{jobId}.csv");
        }
        else
        {
            // Default to Excel
            using var package = new OfficeOpenXml.ExcelPackage();
            var sheet = package.Workbook.Worksheets.Add("Import Report");
            
            sheet.Cells[1, 1].Value = "Type";
            sheet.Cells[1, 2].Value = "Row Number";
            sheet.Cells[1, 3].Value = "SKU";
            sheet.Cells[1, 4].Value = "Field";
            sheet.Cells[1, 5].Value = "Message";

            using (var range = sheet.Cells[1, 1, 1, 5])
            {
                range.Style.Font.Bold = true;
                range.Style.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid;
                range.Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.LightGray);
            }

            int row = 2;
            foreach (var error in result.Errors)
            {
                sheet.Cells[row, 1].Value = "Error";
                sheet.Cells[row, 1].Style.Font.Color.SetColor(System.Drawing.Color.Red);
                sheet.Cells[row, 2].Value = error.RowNumber;
                sheet.Cells[row, 3].Value = error.SKU;
                sheet.Cells[row, 4].Value = error.Field;
                sheet.Cells[row, 5].Value = error.Message;
                row++;
            }

            foreach (var warning in result.Warnings)
            {
                sheet.Cells[row, 1].Value = "Warning";
                sheet.Cells[row, 1].Style.Font.Color.SetColor(System.Drawing.Color.Orange);
                sheet.Cells[row, 2].Value = warning.RowNumber;
                sheet.Cells[row, 3].Value = warning.SKU;
                sheet.Cells[row, 5].Value = warning.Message;
                row++;
            }

            sheet.Cells.AutoFitColumns();

            return File(package.GetAsByteArray(), "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", $"import-report-{jobId}.xlsx");
        }
    }

    /// <summary>
    /// Download sample Excel template
    /// </summary>
    [HttpGet("bulk-upload/template")]
    public IActionResult DownloadTemplate()
    {
        // Return sample template info
        return Ok(new
        {
            Message = "Sample Excel template",
            RequiredColumns = new[]
            {
                "ProductName",
                "CategorySlug",
                "VariantSKU",
                "BasePrice"
            },
            OptionalColumns = new[]
            {
                "Manufacturer",
                "Model",
                "Description",
                "Color",
                "Size",
                "RAM",
                "Storage",
                "CostPrice",
                "StockQuantity",
                "IsActive"
            },
            SampleRow = new
            {
                ProductName = "iPhone 15 Pro",
                CategorySlug = "smartphones",
                Manufacturer = "Apple",
                Model = "iPhone 15 Pro",
                Description = "Latest flagship smartphone",
                VariantSKU = "IP15-256-BLK",
                Color = "Black",
                Size = "256GB",
                RAM = "8GB",
                BasePrice = 47999,
                CostPrice = 38000,
                StockQuantity = 50,
                IsActive = "TRUE"
            }
        });
    }
}
