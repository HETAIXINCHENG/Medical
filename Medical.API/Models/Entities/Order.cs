using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Medical.API.Models.Entities;

/// <summary>
/// 订单
/// </summary>
[Table("Orders")]
public class Order
{
    [Key]
    public Guid Id { get; set; } = Guid.NewGuid();

    [Required]
    public Guid UserId { get; set; }

    [Required]
    [MaxLength(50)]
    public string OrderNo { get; set; } = string.Empty;

    /// <summary>
    /// 订单状态（Pending/Cancelled/Paid/Shipped/Completed/Refunded）
    /// </summary>
    [MaxLength(30)]
    public string Status { get; set; } = "Pending";

    /// <summary>
    /// 支付状态（Unpaid/Paid/Refunding/Refunded/Failed）
    /// </summary>
    [MaxLength(30)]
    public string PayStatus { get; set; } = "Unpaid";

    [Column(TypeName = "decimal(18,2)")]
    public decimal TotalAmount { get; set; } = 0;

    [Column(TypeName = "decimal(18,2)")]
    public decimal PayAmount { get; set; } = 0;

    /// <summary>
    /// 支付方式（WeChat/Alipay/Offline）
    /// </summary>
    [MaxLength(30)]
    public string? PayMethod { get; set; }

    // 收货信息
    [MaxLength(50)]
    public string? Consignee { get; set; }

    [MaxLength(30)]
    public string? Phone { get; set; }

    [MaxLength(50)]
    public string? Province { get; set; }

    [MaxLength(50)]
    public string? City { get; set; }

    [MaxLength(50)]
    public string? District { get; set; }

    [MaxLength(200)]
    public string? AddressLine { get; set; }

    /// <summary>
    /// 一件代发
    /// </summary>
    public bool IsDropship { get; set; } = false;

    [MaxLength(100)]
    public string? DropshipVendor { get; set; }

    [MaxLength(200)]
    public string? Remark { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    public virtual ICollection<OrderItem> Items { get; set; } = new List<OrderItem>();
    public virtual ICollection<Payment> Payments { get; set; } = new List<Payment>();
    public virtual ICollection<Shipment> Shipments { get; set; } = new List<Shipment>();
    public virtual ICollection<Refund> Refunds { get; set; } = new List<Refund>();
}

