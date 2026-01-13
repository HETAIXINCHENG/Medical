using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace Medical.API.Models.Entities;

/// <summary>
/// 三甲医院实体
/// </summary>
[Table("TertiaryHospitals")]
public class TertiaryHospital
{
    [Key]
    public Guid Id { get; set; } = Guid.NewGuid();

    /// <summary>
    /// 医院名称
    /// </summary>
    [Required]
    [MaxLength(100)]
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// 省份ID（外键）
    /// </summary>
    [Required]
    public Guid ProvinceId { get; set; }

    /// <summary>
    /// 城市ID（外键）
    /// </summary>
    [Required]
    public Guid CityId { get; set; }

    /// <summary>
    /// 详细地址
    /// </summary>
    [MaxLength(200)]
    public string? Address { get; set; }

    /// <summary>
    /// 医院等级（三甲、三乙等）
    /// </summary>
    [MaxLength(20)]
    public string Level { get; set; } = "三甲";

    /// <summary>
    /// 医院类型（综合医院、专科医院等）
    /// </summary>
    [MaxLength(50)]
    public string? Type { get; set; }

    /// <summary>
    /// 联系电话
    /// </summary>
    [MaxLength(50)]
    public string? Phone { get; set; }

    /// <summary>
    /// 官网
    /// </summary>
    [MaxLength(500)]
    public string? Website { get; set; }

    /// <summary>
    /// 纬度
    /// </summary>
    [Column(TypeName = "decimal(10,7)")]
    public decimal? Latitude { get; set; }

    /// <summary>
    /// 经度
    /// </summary>
    [Column(TypeName = "decimal(10,7)")]
    public decimal? Longitude { get; set; }

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

    [ForeignKey("CityId")]
    [JsonIgnore]
    public virtual City City { get; set; } = null!;
}

