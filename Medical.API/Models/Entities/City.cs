using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace Medical.API.Models.Entities;

/// <summary>
/// 地级市实体
/// </summary>
[Table("Cities")]
public class City
{
    [Key]
    public Guid Id { get; set; } = Guid.NewGuid();

    /// <summary>
    /// 省份ID（外键）
    /// </summary>
    [Required]
    public Guid ProvinceId { get; set; }

    /// <summary>
    /// 城市代码（行政区划代码）
    /// </summary>
    [Required]
    [MaxLength(10)]
    public string Code { get; set; } = string.Empty;

    /// <summary>
    /// 城市名称
    /// </summary>
    [Required]
    [MaxLength(50)]
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// 简称
    /// </summary>
    [MaxLength(20)]
    public string? ShortName { get; set; }

    /// <summary>
    /// 排序顺序
    /// </summary>
    public int SortOrder { get; set; } = 0;

    /// <summary>
    /// 是否启用
    /// </summary>
    public bool IsEnabled { get; set; } = true;

    /// <summary>
    /// 创建时间
    /// </summary>
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// 更新时间
    /// </summary>
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    // 导航属性
    [ForeignKey("ProvinceId")]
    [JsonIgnore]
    public virtual Province Province { get; set; } = null!;
}

