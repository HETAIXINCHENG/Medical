using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace Medical.API.Models.Entities;

/// <summary>
/// 咨询实体
/// </summary>
[Table("Consultations")]
public class Consultation
{
    [Key]
    public Guid Id { get; set; } = Guid.NewGuid();

    /// <summary>
    /// 医生ID
    /// </summary>
    [Required]
    public Guid DoctorId { get; set; }

    /// <summary>
    /// 咨询类型：Text（图文）、Phone（电话）、Video（视频）、HomeVisit（上门）
    /// </summary>
    [Required]
    [MaxLength(20)]
    public string ConsultationType { get; set; } = "Text";

    /// <summary>
    /// 价格（元）
    /// </summary>
    [Column(TypeName = "decimal(10,2)")]
    public decimal Price { get; set; } = 0;

    /// <summary>
    /// 开始时间
    /// </summary>
    public DateTime? StartTime { get; set; }

    /// <summary>
    /// 结束时间
    /// </summary>
    public DateTime? EndTime { get; set; }

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

    public virtual ICollection<ConsultationMessage> Messages { get; set; } = new List<ConsultationMessage>();
    
    // 多对多关系：通过 ConsultationPatient 关联表
    public virtual ICollection<ConsultationPatient> ConsultationPatients { get; set; } = new List<ConsultationPatient>();
}

