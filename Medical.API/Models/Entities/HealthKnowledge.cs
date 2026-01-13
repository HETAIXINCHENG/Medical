using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Medical.API.Models.Entities;

/// <summary>
/// 健康知识实体
/// </summary>
[Table("HealthKnowledge")]
public class HealthKnowledge
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
    /// 内容
    /// </summary>
    [Required]
    [Column(TypeName = "text")]
    public string Content { get; set; } = string.Empty;

    /// <summary>
    /// 摘要
    /// </summary>
    [MaxLength(500)]
    public string? Summary { get; set; }

    /// <summary>
    /// 封面图片URL
    /// </summary>
    [MaxLength(500)]
    public string? CoverImageUrl { get; set; }

    /// <summary>
    /// 作者(医生)
    /// </summary>
    [MaxLength(100)]
    public string? Author { get; set; }

    /// <summary>
    /// 来源
    /// </summary>
    [MaxLength(100)]
    public string? Source { get; set; }

    /// <summary>
    /// 分类：热门推荐、养生课堂、精彩专题、专家讲堂、健康
    /// </summary>
    [MaxLength(50)]
    public string? Category { get; set; }

    /// <summary>
    /// 阅读量
    /// </summary>
    public int ReadCount { get; set; } = 0;

    /// <summary>
    /// 收藏量
    /// </summary>
    public int FavoriteCount { get; set; } = 0;

    /// <summary>
    /// 是否热门
    /// </summary>
    public bool IsHot { get; set; } = false;

    /// <summary>
    /// 是否推荐
    /// </summary>
    public bool IsRecommended { get; set; } = false;

    /// <summary>
    /// 创建时间
    /// </summary>
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// 更新时间
    /// </summary>
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}

