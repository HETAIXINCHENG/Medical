using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Medical.API.Models.Enums;

namespace Medical.API.Models.Entities;

/// <summary>
/// 家庭成员实体
/// </summary>
[Table("FamilyMembers")]
public class FamilyMember
{
    [Key]
    public Guid Id { get; set; } = Guid.NewGuid();

    /// <summary>
    /// 用户ID
    /// </summary>
    [Required]
    public Guid PatientId { get; set; }

    /// <summary>
    /// 姓名
    /// </summary>
    [Required]
    [MaxLength(50)]
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// 关系类型：配偶、子女、父母、其他
    /// </summary>
    [Required]
    public RelationshipType Relationship { get; set; } = RelationshipType.Other;

    /// <summary>
    /// 性别：Male、Female
    /// </summary>
    [MaxLength(10)]
    public string? Gender { get; set; }

    /// <summary>
    /// 出生日期（加密存储）
    /// </summary>
    public DateTime? DateOfBirth { get; set; }

    /// <summary>
    /// 身份证号
    /// </summary>
    [MaxLength(50)]
    public string? IdCardNumber { get; set; }

    /// <summary>
    /// 手机号
    /// </summary>
    [MaxLength(50)]
    public string? PhoneNumber { get; set; }

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
    public virtual Patient Patient { get; set; } = null!;
}

