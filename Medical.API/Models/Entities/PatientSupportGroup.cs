using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Medical.API.Models.Entities;

/// <summary>
/// 患友会实体
/// </summary>
[Table("PatientSupportGroups")]
public class PatientSupportGroup
{
    [Key]
    public Guid Id { get; set; } = Guid.NewGuid();

    /// <summary>
    /// 医生ID（患友会创建者）
    /// </summary>
    [Required]
    public Guid DoctorId { get; set; }

    /// <summary>
    /// 患友会名称
    /// </summary>
    [Required]
    [MaxLength(100)]
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// 患友会描述
    /// </summary>
    [MaxLength(1000)]
    public string? Description { get; set; }

    /// <summary>
    /// 发布人数（发布过帖子的用户数）
    /// </summary>
    public int PostCount { get; set; } = 0;

    /// <summary>
    /// 阅读总数（所有帖子的阅读数总和）
    /// </summary>
    public int TotalReadCount { get; set; } = 0;

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
    [ForeignKey("DoctorId")]
    public virtual Doctor Doctor { get; set; } = null!;

    public virtual ICollection<Post> Posts { get; set; } = new List<Post>();
    public virtual GroupRules? Rules { get; set; }
}

