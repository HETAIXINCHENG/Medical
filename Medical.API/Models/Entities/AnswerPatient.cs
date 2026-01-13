using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Medical.API.Models.Entities;

/// <summary>
/// 回答-患者关联表（多对多）
/// </summary>
[Table("AnswerPatients")]
public class AnswerPatient
{
    [Key]
    public Guid Id { get; set; } = Guid.NewGuid();

    /// <summary>
    /// 回答ID
    /// </summary>
    [Required]
    public Guid AnswerId { get; set; }

    /// <summary>
    /// 患者ID
    /// </summary>
    [Required]
    public Guid PatientId { get; set; }

    /// <summary>
    /// 创建时间
    /// </summary>
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // 导航属性
    [ForeignKey("AnswerId")]
    public virtual Answer Answer { get; set; } = null!;

    [ForeignKey("PatientId")]
    public virtual Patient Patient { get; set; } = null!;
}

