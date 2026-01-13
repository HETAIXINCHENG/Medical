using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Medical.API.Models.Entities;

/// <summary>
/// 商品
/// </summary>
[Table("Products")]
public class Product
{
    [Key]
    public Guid Id { get; set; } = Guid.NewGuid();

    [Required]
    public Guid CategoryId { get; set; }

    [Required]
    [MaxLength(200)]
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// 商品编码/SKU
    /// </summary>
    [Required]
    [MaxLength(100)]
    public string Code { get; set; } = string.Empty;

    [MaxLength(500)]
    public string? Description { get; set; }

    /// <summary>
    /// 基础售价
    /// </summary>
    [Column(TypeName = "decimal(18,2)")]
    public decimal Price { get; set; } = 0;

    /// <summary>
    /// 市场价
    /// </summary>
    [Column(TypeName = "decimal(18,2)")]
    public decimal? MarketPrice { get; set; }

    public bool IsEnabled { get; set; } = true;

    /// <summary>
    /// 是否虚拟商品
    /// </summary>
    public bool IsVirtual { get; set; } = false;

    /// <summary>
    /// 主图
    /// </summary>
    [MaxLength(500)]
    public string? CoverUrl { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    [ForeignKey(nameof(CategoryId))]
    public virtual ProductCategory Category { get; set; } = null!;

    public virtual ICollection<ProductSpec> Specs { get; set; } = new List<ProductSpec>();
}

