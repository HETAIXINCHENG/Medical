using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Medical.API.Models.Entities;

/// <summary>
/// 活动实体
/// </summary>
[Table("Activities")]
public class Activity
{
    [Key]
    public Guid Id { get; set; } = Guid.NewGuid();

    /// <summary>
    /// 标题
    /// </summary>
    [Required]
    [MaxLength(200)]
    public string Title { get; set; } = string.Empty;

    /// <summary>
    /// 副标题
    /// </summary>
    [MaxLength(200)]
    public string? Subtitle { get; set; }

    /// <summary>
    /// 描述
    /// </summary>
    [MaxLength(1000)]
    public string? Description { get; set; }

    /// <summary>
    /// 封面图片URL
    /// </summary>
    [MaxLength(1000)]
    public string? CoverImageUrl { get; set; }

    /// <summary>
    /// 活动类型：Discount（优惠）、Knowledge（知识）、Product（产品）
    /// </summary>
    [MaxLength(50)]
    public string? ActivityType { get; set; }

    /// <summary>
    /// 优惠信息（如：立减200）
    /// </summary>
    [MaxLength(100)]
    public string? DiscountInfo { get; set; }

    /// <summary>
    /// 是否热门
    /// </summary>
    public bool IsHot { get; set; } = false;

    /// <summary>
    /// 是否大卡片显示
    /// </summary>
    public bool IsLargeCard { get; set; } = false;

    /// <summary>
    /// 排序顺序
    /// </summary>
    public int SortOrder { get; set; } = 0;

    /// <summary>
    /// 开始时间
    /// </summary>
    public DateTime? StartTime { get; set; }

    /// <summary>
    /// 结束时间
    /// </summary>
    public DateTime? EndTime { get; set; }

    /// <summary>
    /// 创建时间
    /// </summary>
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// 更新时间
    /// </summary>
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}

