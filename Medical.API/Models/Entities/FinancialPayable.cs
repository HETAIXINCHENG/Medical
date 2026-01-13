using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Medical.API.Models.Entities;

/// <summary>
/// 财务应付
/// </summary>
[Table("FinancialPayables")]
public class FinancialPayable
{
    [Key]
    public Guid Id { get; set; } = Guid.NewGuid();

    /// <summary>
    /// 关联订单ID（可选）
    /// </summary>
    public Guid? OrderId { get; set; }

    /// <summary>
    /// 参考号（采购单号/物流单号）
    /// </summary>
    [MaxLength(100)]
    public string? ReferenceNo { get; set; }

    /// <summary>
    /// 供应商/结算对象
    /// </summary>
    [MaxLength(100)]
    public string? VendorName { get; set; }

    /// <summary>
    /// 费用类型（采购/运费/平台/其他）
    /// </summary>
    [MaxLength(50)]
    public string? ExpenseType { get; set; }

    /// <summary>
    /// 应付金额
    /// </summary>
    [Column(TypeName = "decimal(18,2)")]
    public decimal Amount { get; set; }

    /// <summary>
    /// 已付金额
    /// </summary>
    [Column(TypeName = "decimal(18,2)")]
    public decimal PaidAmount { get; set; }

    /// <summary>
    /// 待付金额
    /// </summary>
    [Column(TypeName = "decimal(18,2)")]
    public decimal PendingAmount { get; set; }

    /// <summary>
    /// 状态（待付/已付/部分付/已关闭）
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

