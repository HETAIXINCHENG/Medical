using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Medical.API.Models.Entities;

/// <summary>
/// 财务杂费/费用
/// </summary>
[Table("FinancialFees")]
public class FinancialFee
{
    [Key]
    public Guid Id { get; set; } = Guid.NewGuid();

    /// <summary>
    /// 关联订单ID（可选）
    /// </summary>
    public Guid? OrderId { get; set; }

    /// <summary>
    /// 参考号（可为结算批次/订单号）
    /// </summary>
    [MaxLength(100)]
    public string? ReferenceNo { get; set; }

    /// <summary>
    /// 费用类型（平台佣金/营销/对账差异/额外运费等）
    /// </summary>
    [MaxLength(50)]
    public string? FeeType { get; set; }

    /// <summary>
    /// 费用金额（正为支出，负为补贴/减免）
    /// </summary>
    [Column(TypeName = "decimal(18,2)")]
    public decimal Amount { get; set; }

    /// <summary>
    /// 备注
    /// </summary>
    [MaxLength(500)]
    public string? Remark { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}

