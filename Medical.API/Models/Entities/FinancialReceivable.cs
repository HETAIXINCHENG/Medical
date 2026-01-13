using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Medical.API.Models.Entities;

/// <summary>
/// 财务应收
/// </summary>
[Table("FinancialReceivables")]
public class FinancialReceivable
{
    [Key]
    public Guid Id { get; set; } = Guid.NewGuid();

    /// <summary>
    /// 关联订单ID（可选）
    /// </summary>
    public Guid? OrderId { get; set; }

    /// <summary>
    /// 参考号（订单号/渠道单号）
    /// </summary>
    [MaxLength(100)]
    public string? ReferenceNo { get; set; }

    /// <summary>
    /// 收款渠道（如微信、支付宝）
    /// </summary>
    [MaxLength(50)]
    public string? Channel { get; set; }

    /// <summary>
    /// 应收金额
    /// </summary>
    [Column(TypeName = "decimal(18,2)")]
    public decimal Amount { get; set; }

    /// <summary>
    /// 已收金额
    /// </summary>
    [Column(TypeName = "decimal(18,2)")]
    public decimal ReceivedAmount { get; set; }

    /// <summary>
    /// 待收金额
    /// </summary>
    [Column(TypeName = "decimal(18,2)")]
    public decimal PendingAmount { get; set; }

    /// <summary>
    /// 状态（待收/已收/部分收/已关闭）
    /// </summary>
    [MaxLength(30)]
    public string Status { get; set; } = "pending";

    /// <summary>
    /// 币种
    /// </summary>
    [MaxLength(10)]
    public string Currency { get; set; } = "CNY";

    /// <summary>
    /// 备注
    /// </summary>
    [MaxLength(500)]
    public string? Remark { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}

