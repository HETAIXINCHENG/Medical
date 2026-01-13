using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Medical.API.Models.Entities;

/// <summary>
/// 商品规格
/// </summary>
[Table("ProductSpecs")]
public class ProductSpec
{
    [Key]
    public Guid Id { get; set; } = Guid.NewGuid();

    [Required]
    public Guid ProductId { get; set; }

    [Required]
    [MaxLength(100)]
    public string SpecName { get; set; } = string.Empty;

    [MaxLength(100)]
    public string? SpecCode { get; set; }

    [Column(TypeName = "decimal(18,2)")]
    public decimal Price { get; set; } = 0;

    /// <summary>
    /// 库存数量
    /// </summary>
    public int Stock { get; set; } = 0;

    /// <summary>
    /// 重量（用于物流计费，可选）
    /// </summary>
    [Column(TypeName = "decimal(18,2)")]
    public decimal? Weight { get; set; }

    public bool IsDefault { get; set; } = false;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    [ForeignKey(nameof(ProductId))]
    public virtual Product Product { get; set; } = null!;

    public virtual ICollection<OrderItem> OrderItems { get; set; } = new List<OrderItem>();
    public virtual ICollection<CartItem> CartItems { get; set; } = new List<CartItem>();
}

