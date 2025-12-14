using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ECommerce.Domain.Entities;

public class CompanyCampaign
{
    [Key]
    public int CampaignId { get; set; }

    [Required]
    public int CompanyId { get; set; }

    [Required]
    [MaxLength(200)]
    public string CampaignName { get; set; } = string.Empty;

    [MaxLength(1000)]
    public string? Description { get; set; }

    [Required]
    [Column(TypeName = "decimal(5,2)")]
    public decimal DiscountPercentage { get; set; }

    [Column(TypeName = "decimal(18,2)")]
    public decimal MinOrderAmount { get; set; } = 0;

    public DateTime ValidFrom { get; set; }

    public DateTime ValidTo { get; set; }

    public bool IsActive { get; set; } = true;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    // Navigation properties
    [ForeignKey(nameof(CompanyId))]
    public virtual Company Company { get; set; } = null!;
}
