using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ECommerce.Domain.Entities;

public class CompanyApprovalRule
{
    [Key]
    public int RuleId { get; set; }

    [Required]
    public int CompanyId { get; set; }

    [Required]
    [MaxLength(200)]
    public string RuleName { get; set; } = string.Empty;

    [Required]
    [Column(TypeName = "decimal(18,2)")]
    public decimal ThresholdAmount { get; set; }

    public int? CategoryId { get; set; }

    [Required]
    [MaxLength(100)]
    public string ApproverRole { get; set; } = string.Empty;

    public int ApprovalLevel { get; set; } = 1;

    public bool IsActive { get; set; } = true;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    // Navigation properties
    [ForeignKey(nameof(CompanyId))]
    public virtual Company Company { get; set; } = null!;

    [ForeignKey(nameof(CategoryId))]
    public virtual Category? Category { get; set; }
}
