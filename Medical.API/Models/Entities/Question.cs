using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace Medical.API.Models.Entities;

/// <summary>
/// 问题实体
/// </summary>
[Table("Questions")]
public class Question
{
    [Key]
    public Guid Id { get; set; } = Guid.NewGuid();

    /// <summary>
    /// 患者ID（提问者）
    /// </summary>
    [Required]
    public Guid PatientId { get; set; }

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
    /// 分类：热门、养生、问诊、保健、减肥、育儿类、失眠类等
    /// </summary>
    [MaxLength(50)]
    public string? Category { get; set; }

    /// <summary>
    /// 标签
    /// </summary>
    [MaxLength(200)]
    public string? Tags { get; set; }

    /// <summary>
    /// 收藏数
    /// </summary>
    public int FavoriteCount { get; set; } = 0;

    /// <summary>
    /// 回答数
    /// </summary>
    public int AnswerCount { get; set; } = 0;

    /// <summary>
    /// 查看数
    /// </summary>
    public int ViewCount { get; set; } = 0;

    /// <summary>
    /// 是否热门
    /// </summary>
    public bool IsHot { get; set; } = false;

    /// <summary>
    /// 创建时间
    /// </summary>
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// 更新时间
    /// </summary>
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    // 导航属性
    [ForeignKey("PatientId")]
    [JsonIgnore]
    public virtual Patient Patient { get; set; } = null!;

    [JsonIgnore]
    public virtual ICollection<Answer> Answers { get; set; } = new List<Answer>();
}

