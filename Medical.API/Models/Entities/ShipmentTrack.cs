using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Medical.API.Models.Entities;

/// <summary>
/// 物流轨迹
/// </summary>
[Table("ShipmentTracks")]
public class ShipmentTrack
{
    [Key]
    public Guid Id { get; set; } = Guid.NewGuid();

    [Required]
    public Guid ShipmentId { get; set; }

    [MaxLength(200)]
    public string? StatusText { get; set; }

    [MaxLength(100)]
    public string? Location { get; set; }

    public DateTime OccurredAt { get; set; } = DateTime.UtcNow;

    public string? RawPayload { get; set; }

    [ForeignKey(nameof(ShipmentId))]
    public virtual Shipment Shipment { get; set; } = null!;
}

