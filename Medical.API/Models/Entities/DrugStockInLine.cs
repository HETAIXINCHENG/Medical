using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace Medical.API.Models.Entities;

/// <summary>
/// 药品入库单行表（存储一次入库操作中，每一种药品的具体明细）
/// </summary>
[Table("DrugStockInLines")]
public class DrugStockInLine
{
    [Key]
    public Guid Id { get; set; } = Guid.NewGuid();

    /// <summary>
    /// 关联的入库单头ID
    /// </summary>
    [Required]
    public Guid HeadId { get; set; }

    /// <summary>
    /// 药品ID
    /// </summary>
    [Required]
    public Guid DrugId { get; set; }

    /// <summary>
    /// 生产批号（用于追溯）
    /// </summary>
    [Required]
    [MaxLength(50)]
    public string BatchNumber { get; set; } = string.Empty;

    /// <summary>
    /// 生产日期
    /// </summary>
    public DateTime? ProductionDate { get; set; }

    /// <summary>
    /// 有效期至（用于近效期预警）
    /// </summary>
    [Required]
    public DateTime ExpiryDate { get; set; }

    /// <summary>
    /// 本次入库数量
    /// </summary>
    [Required]
    public int Quantity { get; set; }

    /// <summary>
    /// 单价（进价/成本价）
    /// </summary>
    [Required]
    [Column(TypeName = "decimal(10,2)")]
    public decimal PurchasePrice { get; set; }

    /// <summary>
    /// 小计金额（quantity * purchase_price）
    /// </summary>
    [Required]
    [Column(TypeName = "decimal(15,2)")]
    public decimal Subtotal { get; set; }

    /// <summary>
    /// 库位/仓库
    /// </summary>
    [Required]
    [MaxLength(50)]
    public string WarehouseLocation { get; set; } = "主仓库";

    /// <summary>
    /// 创建时间
    /// </summary>
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // 导航属性
    [ForeignKey("HeadId")]
    [JsonIgnore]
    public virtual DrugStockInHead Head { get; set; } = null!;

    [ForeignKey("DrugId")]
    [JsonIgnore]
    public virtual Drug Drug { get; set; } = null!;
}

