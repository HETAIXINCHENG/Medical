using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Medical.API.Models.Entities;

/// <summary>
/// 用户实体
/// </summary>
[Table("Users")]
public class User
{
    [Key]
    public Guid Id { get; set; } = Guid.NewGuid();

    /// <summary>
    /// 用户名
    /// </summary>
    [Required]
    [MaxLength(50)]
    public string Username { get; set; } = string.Empty;

    /// <summary>
    /// 密码哈希（加密存储）
    /// </summary>
    [Required]
    [MaxLength(255)]
    public string PasswordHash { get; set; } = string.Empty;

    /// <summary>
    /// 手机号
    /// </summary>
    [MaxLength(50)]
    public string? PhoneNumber { get; set; }

    /// <summary>
    /// 邮箱
    /// </summary>
    [MaxLength(50)]
    public string? Email { get; set; }

    /// <summary>
    /// 头像URL
    /// </summary>
    [MaxLength(500)]
    public string? AvatarUrl { get; set; }

    /// <summary>
    /// 简介
    /// </summary>
    [MaxLength(500)]
    public string? Bio { get; set; }

    /// <summary>
    /// 角色：Patient（患者）、Doctor（医生）、Admin（管理员）
    /// </summary>
    [Required]
    [MaxLength(20)]
    public string Role { get; set; } = "Patient";

    /// <summary>
    /// 用户类型ID（关联到UserTypeDictionary表）
    /// 1=System, 2=Doctor, 3=Patient
    /// </summary>
    [Required]
    public int UserTypeId { get; set; } = 3; // 默认为Patient

    /// <summary>
    /// 是否激活
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
    


    /// <summary>
    /// 设定删除标志，假删除用户（永久封禁）
    /// </summary>
    public bool IsDeleted { get; set; } = false;


    // 导航属性
    [ForeignKey("UserTypeId")]
    public virtual UserTypeDictionary? UserType { get; set; }
    
    [System.Text.Json.Serialization.JsonIgnore]
    public virtual ICollection<UserRole> UserRoles { get; set; } = new List<UserRole>();
    
    // 导航属性：User 只保留与自身直接相关的导航属性
    // 注意：Consultations, Answers, Questions, DoctorReviews, Prescriptions, VisitRecords, ExaminationReports 等
    // 现在都关联到 Patient 而不是 User，所以不再需要这些导航属性
    
    // 患友会相关导航属性（医生和患者都可以发帖、评论、点赞）
    public virtual ICollection<Post> Posts { get; set; } = new List<Post>();
    public virtual ICollection<PostComment> PostComments { get; set; } = new List<PostComment>();
    public virtual ICollection<PostLike> PostLikes { get; set; } = new List<PostLike>();
}

