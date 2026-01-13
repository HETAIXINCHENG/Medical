using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace Medical.API.Models.Entities;

/// <summary>
/// 检查报告实体
/// </summary>
[Table("ExaminationReports")]
public class ExaminationReport
{
    [Key]
    public Guid Id { get; set; } = Guid.NewGuid();

    /// <summary>
    /// 患者ID
    /// </summary>
    [Required]
    public Guid PatientId { get; set; }

    /// <summary>
    /// 医生ID（开具检查的医生）
    /// </summary>
    [Required]
    public Guid DoctorId { get; set; }

    /// <summary>
    /// 就诊记录ID
    /// </summary>
    public Guid? VisitRecordId { get; set; }

    /// <summary>
    /// 咨询ID
    /// </summary>
    public Guid? ConsultationId { get; set; }

    /// <summary>
    /// 报告编号
    /// </summary>
    [Required]
    [MaxLength(50)]
    public string ReportNumber { get; set; } = string.Empty;

    /// <summary>
    /// 检查类型：BloodTest（血常规）、UrineTest（尿常规）、XRay（X光）、CT（CT）、MRI（核磁共振）、Ultrasound（B超）、ECG（心电图）等
    /// </summary>
    [Required]
    [MaxLength(50)]
    public string ExaminationType { get; set; } = string.Empty;

    /// <summary>
    /// 检查项目名称
    /// </summary>
    [Required]
    [MaxLength(200)]
    public string ExaminationName { get; set; } = string.Empty;

    /// <summary>
    /// 检查日期
    /// </summary>
    [Required]
    public DateTime ExaminationDate { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// 报告日期
    /// </summary>
    [Required]
    public DateTime ReportDate { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// 检查结果（JSON格式存储）
    /// </summary>
    [Required]
    [Column(TypeName = "text")]
    public string Results { get; set; } = string.Empty;

    /// <summary>
    /// 检查结论
    /// </summary>
    [MaxLength(1000)]
    public string? Conclusion { get; set; }

    /// <summary>
    /// 建议
    /// </summary>
    [Column(TypeName = "text")]
    public string? Recommendations { get; set; }

    /// <summary>
    /// 报告文件URL（PDF、图片等）
    /// </summary>
    [MaxLength(500)]
    public string? ReportFileUrl { get; set; }

    /// <summary>
    /// 状态：Pending（待检查）、Completed（已完成）、Cancelled（已取消）
    /// </summary>
    [Required]
    [MaxLength(20)]
    public string Status { get; set; } = "Pending";

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

    [ForeignKey("VisitRecordId")]
    [JsonIgnore]
    public virtual VisitRecord? VisitRecord { get; set; }

    [ForeignKey("ConsultationId")]
    [JsonIgnore]
    public virtual Consultation? Consultation { get; set; }
}

