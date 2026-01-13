using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace Medical.API.Models.Entities;

/// <summary>
/// 药品库存实体（动态库存，支持多批次、多仓库）
/// </summary>
[Table("DrugInventories")]
public class DrugInventory
{
    [Key]
    public Guid Id { get; set; } = Guid.NewGuid();

    /// <summary>
    /// 药品ID
    /// </summary>
    [Required]
    public Guid DrugId { get; set; }

    /// <summary>
    /// 库位/仓库（支持分库位管理）
    /// </summary>
    [Required]
    [MaxLength(50)]
    public string WarehouseLocation { get; set; } = "主仓库";

    /// <summary>
    /// 当前库存数量（通过入库和出库操作更新）
    /// </summary>
    [Required]
    public int CurrentQuantity { get; set; } = 0;

    /// <summary>
    /// 库存最后更新时间
    /// </summary>
    public DateTime LastUpdatedAt { get; set; } = DateTime.UtcNow;

    // 导航属性
    [ForeignKey("DrugId")]
    [JsonIgnore]
    public virtual Drug Drug { get; set; } = null!;
}

