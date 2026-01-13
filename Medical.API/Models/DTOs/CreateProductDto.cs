using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Medical.API.Models.DTOs;

/// <summary>
/// 创建商品DTO
/// </summary>
public class CreateProductDto
{
    [Required(ErrorMessage = "商品分类不能为空")]
    public Guid CategoryId { get; set; }

    [Required(ErrorMessage = "商品名称不能为空")]
    [MaxLength(200)]
    public string Name { get; set; } = string.Empty;

    [Required(ErrorMessage = "商品编码不能为空")]
    [MaxLength(100)]
    public string Code { get; set; } = string.Empty;

    [MaxLength(500)]
    public string? Description { get; set; }

    [Column(TypeName = "decimal(18,2)")]
    public decimal Price { get; set; } = 0;

    [Column(TypeName = "decimal(18,2)")]
    public decimal? MarketPrice { get; set; }

    public bool IsEnabled { get; set; } = true;

    public bool IsVirtual { get; set; } = false;

    [MaxLength(500)]
    public string? CoverUrl { get; set; }
}

