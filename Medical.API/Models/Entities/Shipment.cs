using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Medical.API.Models.Entities;

/// <summary>
/// 发货单（支持一单多包裹）
/// </summary>
[Table("Shipments")]
public class Shipment
{
    [Key]
    public Guid Id { get; set; } = Guid.NewGuid();

    [Required]
    public Guid OrderId { get; set; }

    [Required]
    public Guid ShipCompanyId { get; set; }

    [Required]
    [MaxLength(100)]
    public string TrackingNo { get; set; } = string.Empty;

    /// <summary>
    /// 包裹序号（多包裹时用于排序）
    /// </summary>
    public int PackageIndex { get; set; } = 1;

    public DateTime? ShippedAt { get; set; }
    public DateTime? DeliveredAt { get; set; }

    /// <summary>
    /// 状态（Pending/Shipped/InTransit/Delivered/Exception）
    /// </summary>
    [MaxLength(30)]
    public string Status { get; set; } = "Pending";

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    [ForeignKey(nameof(OrderId))]
    public virtual Order Order { get; set; } = null!;

    [ForeignKey(nameof(ShipCompanyId))]
    public virtual ShipCompany ShipCompany { get; set; } = null!;

    public virtual ICollection<ShipmentTrack> Tracks { get; set; } = new List<ShipmentTrack>();
}

