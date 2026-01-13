using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace Medical.API.Models.Entities;

/// <summary>
/// 就诊记录实体
/// </summary>
[Table("VisitRecords")]
public class VisitRecord
{
    [Key]
    public Guid Id { get; set; } = Guid.NewGuid();

    /// <summary>
    /// 患者ID
    /// </summary>
    [Required]
    public Guid PatientId { get; set; }

    /// <summary>
    /// 医生ID
    /// </summary>
    [Required]
    public Guid DoctorId { get; set; }

    /// <summary>
    /// 咨询ID（关联的咨询）
    /// </summary>
    public Guid? ConsultationId { get; set; }

    /// <summary>
    /// 就诊日期
    /// </summary>
    [Required]
    public DateTime VisitDate { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// 主诉（患者描述的症状）
    /// </summary>
    [MaxLength(1000)]
    public string? ChiefComplaint { get; set; }

    /// <summary>
    /// 现病史
    /// </summary>
    [Column(TypeName = "text")]
    public string? PresentIllness { get; set; }

    /// <summary>
    /// 诊断结果
    /// </summary>
    [MaxLength(1000)]
    public string? Diagnosis { get; set; }

    /// <summary>
    /// 治疗方案
    /// </summary>
    [Column(TypeName = "text")]
    public string? TreatmentPlan { get; set; }

    /// <summary>
    /// 医嘱
    /// </summary>
    [Column(TypeName = "text")]
    public string? MedicalAdvice { get; set; }

    /// <summary>
    /// 就诊类型：Outpatient（门诊）、Inpatient（住院）、Emergency（急诊）、FollowUp（复诊）
    /// </summary>
    [Required]
    [MaxLength(20)]
    public string VisitType { get; set; } = "Outpatient";

    /// <summary>
    /// 状态：Completed（已完成）、InProgress（进行中）、Cancelled（已取消）
    /// </summary>
    [Required]
    [MaxLength(20)]
    public string Status { get; set; } = "Completed";

    /// <summary>
    /// 备注
    /// </summary>
    [MaxLength(1000)]
    public string? Notes { get; set; }

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
    [JsonIgnore]
    public virtual Patient Patient { get; set; } = null!;

    [ForeignKey("DoctorId")]
    [JsonIgnore]
    public virtual Doctor Doctor { get; set; } = null!;

    [ForeignKey("ConsultationId")]
    [JsonIgnore]
    public virtual Consultation? Consultation { get; set; }
}

