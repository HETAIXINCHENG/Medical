using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Medical.API.Models.Entities;

/// <summary>
/// 患友会会规实体
/// </summary>
[Table("GroupRules")]
public class GroupRules
{
    [Key]
    public Guid Id { get; set; } = Guid.NewGuid();

    /// <summary>
    /// 患友会ID
    /// </summary>
    [Required]
    public Guid PatientSupportGroupId { get; set; }

    /// <summary>
    /// 会规内容
    /// </summary>
    [Required]
    [Column(TypeName = "text")]
    public string Content { get; set; } = string.Empty;

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
}

