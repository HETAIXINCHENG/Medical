using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Medical.API.Models.Entities;

/// <summary>
/// 支付记录
/// </summary>
[Table("Payments")]
public class Payment
{
    [Key]
    public Guid Id { get; set; } = Guid.NewGuid();

    [Required]
    public Guid OrderId { get; set; }

    /// <summary>
    /// 支付方式（WeChat/Alipay/Offline）
    /// </summary>
    [MaxLength(30)]
    public string PayMethod { get; set; } = "WeChat";

    [Column(TypeName = "decimal(18,2)")]
    public decimal Amount { get; set; } = 0;

    public DateTime? PayTime { get; set; }

    /// <summary>
    /// 渠道交易号
    /// </summary>
    [MaxLength(100)]
    public string? PayTxnId { get; set; }

    /// <summary>
    /// 支付网关回调原文
    /// </summary>
    public string? PayChannelRaw { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    [ForeignKey(nameof(OrderId))]
    public virtual Order Order { get; set; } = null!;
}

