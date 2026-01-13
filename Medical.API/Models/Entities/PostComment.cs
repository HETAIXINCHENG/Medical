using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Medical.API.Models.Entities;

/// <summary>
/// 帖子评论实体
/// </summary>
[Table("PostComments")]
public class PostComment
{
    [Key]
    public Guid Id { get; set; } = Guid.NewGuid();

    /// <summary>
    /// 帖子ID
    /// </summary>
    [Required]
    public Guid PostId { get; set; }

    /// <summary>
    /// 评论者ID（用户ID，可以是患者或医生）
    /// </summary>
    [Required]
    public Guid UserId { get; set; }

    /// <summary>
    /// 评论内容
    /// </summary>
    [Required]
    [MaxLength(2000)]
    public string Content { get; set; } = string.Empty;

    /// <summary>
    /// 附件URL列表（JSON格式存储多个图片/视频URL）
    /// </summary>
    [Column(TypeName = "text")]
    public string? AttachmentUrls { get; set; }

    /// <summary>
    /// 父评论ID（用于回复）
    /// </summary>
    public Guid? ParentCommentId { get; set; }

    /// <summary>
    /// 是否删除
    /// </summary>
    public bool IsDeleted { get; set; } = false;

    /// <summary>
    /// 创建时间
    /// </summary>
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// 更新时间
    /// </summary>
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    // 导航属性
    [ForeignKey("PostId")]
    public virtual Post Post { get; set; } = null!;

    [ForeignKey("UserId")]
    public virtual User User { get; set; } = null!;

    [ForeignKey("ParentCommentId")]
    public virtual PostComment? ParentComment { get; set; }

    public virtual ICollection<PostComment> Replies { get; set; } = new List<PostComment>();
}

