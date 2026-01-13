using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Medical.API.Models.Entities;

/// <summary>
/// 帖子实体
/// </summary>
[Table("Posts")]
public class Post
{
    [Key]
    public Guid Id { get; set; } = Guid.NewGuid();

    /// <summary>
    /// 患友会ID
    /// </summary>
    [Required]
    public Guid PatientSupportGroupId { get; set; }

    /// <summary>
    /// 发布者ID（用户ID，可以是患者或医生）
    /// </summary>
    [Required]
    public Guid UserId { get; set; }

    /// <summary>
    /// 附件URL列表（JSON格式存储多个图片/视频URL）
    /// </summary>
    [Column(TypeName = "text")]
    public string? AttachmentUrls { get; set; }

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
    /// 标签类型：求助、分享、讨论等
    /// </summary>
    [MaxLength(20)]
    public string? Tag { get; set; }

    /// <summary>
    /// 阅读数
    /// </summary>
    public int ReadCount { get; set; } = 0;

    /// <summary>
    /// 评论数
    /// </summary>
    public int CommentCount { get; set; } = 0;

    /// <summary>
    /// 点赞数
    /// </summary>
    public int LikeCount { get; set; } = 0;

    /// <summary>
    /// 最后回复时间
    /// </summary>
    public DateTime? LastReplyAt { get; set; }

    /// <summary>
    /// 是否置顶
    /// </summary>
    public bool IsPinned { get; set; } = false;

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
    [ForeignKey("PatientSupportGroupId")]
    public virtual PatientSupportGroup PatientSupportGroup { get; set; } = null!;

    [ForeignKey("UserId")]
    public virtual User User { get; set; } = null!;

    public virtual ICollection<PostComment> Comments { get; set; } = new List<PostComment>();
    public virtual ICollection<PostLike> Likes { get; set; } = new List<PostLike>();
}

