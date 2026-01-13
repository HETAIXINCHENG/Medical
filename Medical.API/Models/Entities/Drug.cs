using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace Medical.API.Models.Entities;

/// <summary>
/// 药品信息实体（核心字典表）
/// </summary>
[Table("Drugs")]
public class Drug
{
    [Key]
    public Guid Id { get; set; } = Guid.NewGuid();

    /// <summary>
    /// 药品通用名（如：阿莫西林），主要搜索字段
    /// </summary>
    [Required]
    [MaxLength(100)]
    public string CommonName { get; set; } = string.Empty;

    /// <summary>
    /// 药品商品名（如：阿莫仙），可有多个
    /// </summary>
    [MaxLength(100)]
    public string? TradeName { get; set; }

    /// <summary>
    /// 药品规格（如：0.25g*24粒/盒）
    /// </summary>
    [Required]
    [MaxLength(50)]
    public string Specification { get; set; } = string.Empty;

    /// <summary>
    /// 生产厂家
    /// </summary>
    [Required]
    [MaxLength(100)]
    public string Manufacturer { get; set; } = string.Empty;

    /// <summary>
    /// 国药准字批准文号，唯一标识，必须唯一
    /// </summary>
    [Required]
    [MaxLength(50)]
    public string ApprovalNumber { get; set; } = string.Empty;

    /// <summary>
    /// 所属分类ID
    /// </summary>
    [Required]
    public Guid CategoryId { get; set; }

    /// <summary>
    /// 药品单位（如：盒、瓶、支）
    /// </summary>
    [Required]
    [MaxLength(20)]
    public string Unit { get; set; } = string.Empty;

    /// <summary>
    /// 储存条件（如：常温、阴凉、冷藏、冷冻）
    /// </summary>
    [MaxLength(50)]
    public string StorageCondition { get; set; } = "常温";

    /// <summary>
    /// 是否启用（用于逻辑删除，避免误删基础数据）
    /// </summary>
    public bool IsActive { get; set; } = true;

    /// <summary>
    /// 创建时间
    /// </summary>
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// 更新时间
    /// </summary>
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    // 导航属性
    [ForeignKey("CategoryId")]
    [JsonIgnore]
    public virtual DrugCategory Category { get; set; } = null!;

    [JsonIgnore]
    public virtual ICollection<DrugInventory> Inventories { get; set; } = new List<DrugInventory>();

    [JsonIgnore]
    public virtual ICollection<DrugStockInLine> StockInLines { get; set; } = new List<DrugStockInLine>();
}

