using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Medical.API.Models.Entities;

/// <summary>
/// 医生-患者关联表（多对多）
/// </summary>
[Table("DoctorPatients")]
public class DoctorPatient
{
    [Key]
    public Guid Id { get; set; } = Guid.NewGuid();

    /// <summary>
    /// 医生ID
    /// </summary>
    [Required]
    public Guid DoctorId { get; set; }

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
    [ForeignKey("DoctorId")]
    public virtual Doctor Doctor { get; set; } = null!;

    [ForeignKey("PatientId")]
    public virtual Patient Patient { get; set; } = null!;
}

