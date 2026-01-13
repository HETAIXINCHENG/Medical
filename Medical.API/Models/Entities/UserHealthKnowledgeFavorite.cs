using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Medical.API.Models.Entities;

/// <summary>
/// 用户健康知识收藏记录
/// </summary>
[Table("UserHealthKnowledgeFavorites")]
public class UserHealthKnowledgeFavorite
{
    [Key]
    public Guid Id { get; set; } = Guid.NewGuid();

    /// <summary>
    /// 用户ID
    /// </summary>
    [Required]
    public Guid UserId { get; set; }

    /// <summary>
    /// 健康知识ID
    /// </summary>
    [Required]
    public Guid HealthKnowledgeId { get; set; }

    /// <summary>
    /// 收藏时间
    /// </summary>
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // 导航属性
    [ForeignKey("UserId")]
    public virtual User User { get; set; } = null!;

    [ForeignKey("HealthKnowledgeId")]
    public virtual HealthKnowledge HealthKnowledge { get; set; } = null!;
}

