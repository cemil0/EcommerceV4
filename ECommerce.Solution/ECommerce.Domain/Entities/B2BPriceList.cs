using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ECommerce.Domain.Entities;

public class B2BPriceList
{
    [Key]
    public int PriceListId { get; set; }

    [Required]
    [MaxLength(100)]
    public string PriceListName { get; set; } = string.Empty;

    [MaxLength(500)]
    public string? Description { get; set; }

    public bool IsActive { get; set; } = true;

    public DateTime ValidFrom { get; set; }

    public DateTime? ValidTo { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    // Navigation properties
    public virtual ICollection<B2BPriceListItem> Items { get; set; } = new List<B2BPriceListItem>();
    public virtual ICollection<Company> Companies { get; set; } = new List<Company>();
}
