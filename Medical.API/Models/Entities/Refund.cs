using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Medical.API.Models.Entities;

/// <summary>
/// 退款记录
/// </summary>
[Table("Refunds")]
public class Refund
{
    [Key]
    public Guid Id { get; set; } = Guid.NewGuid();

    [Required]
    public Guid OrderId { get; set; }

    public Guid? OrderItemId { get; set; }

    [Column(TypeName = "decimal(18,2)")]
    public decimal Amount { get; set; } = 0;

    [MaxLength(200)]
    public string? Reason { get; set; }

    /// <summary>
    /// 退款状态（Pending/Processing/Success/Failed）
    /// </summary>
    [MaxLength(30)]
    public string Status { get; set; } = "Pending";

    /// <summary>
    /// 退款方式（WeChat/Alipay/OriginalRoute）
    /// </summary>
    [MaxLength(50)]
    public string? RefundMethod { get; set; }

    /// <summary>
    /// 渠道退款单号
    /// </summary>
    [MaxLength(100)]
    public string? ChannelRefundNo { get; set; }

    public DateTime? InitiatedAt { get; set; }
    public DateTime? CompletedAt { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    [ForeignKey(nameof(OrderId))]
    public virtual Order Order { get; set; } = null!;

    [ForeignKey(nameof(OrderItemId))]
    public virtual OrderItem? OrderItem { get; set; }
}

