using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ECommerce.Domain.Entities;

public class B2BPriceListItem
{
    [Key]
    public int PriceListItemId { get; set; }

    [Required]
    public int PriceListId { get; set; }

    [Required]
    public int ProductVariantId { get; set; }

    [Required]
    [Column(TypeName = "decimal(18,2)")]
    public decimal B2BPrice { get; set; }

    public int MinQuantity { get; set; } = 1;

    public int? MaxQuantity { get; set; }

    [Column(TypeName = "decimal(5,2)")]
    public decimal DiscountPercentage { get; set; } = 0;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    // Navigation properties
    [ForeignKey(nameof(PriceListId))]
    public virtual B2BPriceList PriceList { get; set; } = null!;

    [ForeignKey(nameof(ProductVariantId))]
    public virtual ProductVariant ProductVariant { get; set; } = null!;
}
