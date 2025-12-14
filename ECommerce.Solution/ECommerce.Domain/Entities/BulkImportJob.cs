using System.ComponentModel.DataAnnotations;

namespace ECommerce.Domain.Entities;

public class BulkImportJob
{
    [Key]
    public Guid JobId { get; set; }
    public string FileName { get; set; } = string.Empty;
    public string Status { get; set; } = "Pending"; // Pending, Processing, Completed, Failed
    public int TotalRows { get; set; }
    public int ProcessedRows { get; set; }
    public int SuccessCount { get; set; }
    public int FailedCount { get; set; }
    public string? ErrorMessage { get; set; }
    public string? ResultJson { get; set; } // Stores the final BulkImportResultDto as JSON
    public Guid UserId { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? StartedAt { get; set; }
    public DateTime? CompletedAt { get; set; }
}
