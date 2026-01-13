using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Medical.API.Models.Entities;

/// <summary>
/// 帖子点赞实体
/// </summary>
[Table("PostLikes")]
public class PostLike
{
    [Key]
    public Guid Id { get; set; } = Guid.NewGuid();

    /// <summary>
    /// 帖子ID
    /// </summary>
    [Required]
    public Guid PostId { get; set; }

    /// <summary>
    /// 点赞者ID（用户ID，可以是患者或医生）
    /// </summary>
    [Required]
    public Guid UserId { get; set; }

    /// <summary>
    /// 创建时间
    /// </summary>
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // 导航属性
    [ForeignKey("PostId")]
    public virtual Post Post { get; set; } = null!;

    [ForeignKey("UserId")]
    public virtual User User { get; set; } = null!;
}

