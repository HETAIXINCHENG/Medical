using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace Medical.API.Models.Entities;

/// <summary>
/// 药品入库单头表（存储一次入库操作的总体信息）
/// </summary>
[Table("DrugStockInHeads")]
public class DrugStockInHead
{
    [Key]
    public Guid Id { get; set; } = Guid.NewGuid();

    /// <summary>
    /// 入库单号/发票号，业务流水号，必须唯一
    /// </summary>
    [Required]
    [MaxLength(50)]
    public string InvoiceNo { get; set; } = string.Empty;

    /// <summary>
    /// 供应商名称
    /// </summary>
    [Required]
    [MaxLength(100)]
    public string SupplierName { get; set; } = string.Empty;

    /// <summary>
    /// 操作员ID
    /// </summary>
    [Required]
    public Guid OperatorId { get; set; }

    /// <summary>
    /// 入库操作时间
    /// </summary>
    [Required]
    public DateTime OperationTime { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// 本次入库的总金额（可选，可由行项目汇总）
    /// </summary>
    [Column(TypeName = "decimal(15,2)")]
    public decimal TotalAmount { get; set; } = 0.00m;

    /// <summary>
    /// 备注信息
    /// </summary>
    [Column(TypeName = "text")]
    public string? Remarks { get; set; }

    /// <summary>
    /// 入库单状态（1-已入库，0-已取消）
    /// </summary>
    [Required]
    public int Status { get; set; } = 1; // 1-已入库，0-已取消

    /// <summary>
    /// 创建时间
    /// </summary>
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// 更新时间
    /// </summary>
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    // 导航属性
    [ForeignKey("OperatorId")]
    [JsonIgnore]
    public virtual User Operator { get; set; } = null!;

    [JsonIgnore]
    public virtual ICollection<DrugStockInLine> Lines { get; set; } = new List<DrugStockInLine>();
}

