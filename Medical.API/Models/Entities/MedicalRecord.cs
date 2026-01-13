using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace Medical.API.Models.Entities;

/// <summary>
/// 病历信息
/// </summary>
[Table("MedicalRecords")]
public class MedicalRecord
{
    [Key]
    public Guid Id { get; set; } = Guid.NewGuid();

    /// <summary>
    /// 患者ID
    /// </summary>
    [Required]
    public Guid PatientId { get; set; }

    /// <summary>
    /// 主诊医生ID（可选）
    /// </summary>
    public Guid? DoctorId { get; set; }

    /// <summary>
    /// 关联的咨询ID（可选）
    /// </summary>
    public Guid? ConsultationId { get; set; }

    /// <summary>
    /// 病历生成时间
    /// </summary>
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// 身高（cm）
    /// </summary>
    public decimal? Height { get; set; }

    /// <summary>
    /// 体重（kg）
    /// </summary>
    public decimal? Weight { get; set; }

    /// <summary>
    /// 本次患病时长描述（如：2天、一周、半年等）
    /// </summary>
    [MaxLength(200)]
    public string? DiseaseDuration { get; set; }

    /// <summary>
    /// 疾病名称/症状
    /// </summary>
    [MaxLength(200)]
    public string? DiseaseName { get; set; }

    /// <summary>
    /// 是否已在医院就诊
    /// </summary>
    public bool? HasVisitedHospital { get; set; }

    /// <summary>
    /// 当前正在使用的药物（描述）
    /// </summary>
    [MaxLength(1000)]
    public string? CurrentMedications { get; set; }

    /// <summary>
    /// 当前是否怀孕（适用女性）
    /// </summary>
    public bool? IsPregnant { get; set; }

    /// <summary>
    /// 手术/放化疗等重大疾病治疗经历及慢性病史
    /// </summary>
    [MaxLength(2000)]
    public string? MajorTreatmentHistory { get; set; }

    /// <summary>
    /// 药物过敏史
    /// </summary>
    [MaxLength(1000)]
    public string? AllergyHistory { get; set; }

    /// <summary>
    /// 详细病情描述
    /// </summary>
    [MaxLength(4000)]
    public string? DiseaseDescription { get; set; }

    /// <summary>
    /// 既往病史
    /// </summary>
    [MaxLength(2000)]
    public string? PastMedicalHistory { get; set; }

    /// <summary>
    /// 其他补充说明
    /// </summary>
    [MaxLength(2000)]
    public string? AdditionalNotes { get; set; }

    // 导航属性
    [ForeignKey("PatientId")]
    [JsonIgnore]
    public virtual Patient Patient { get; set; } = null!;

    [ForeignKey("DoctorId")]
    [JsonIgnore]
    public virtual Doctor? Doctor { get; set; }

    [ForeignKey("ConsultationId")]
    [JsonIgnore]
    public virtual Consultation? Consultation { get; set; }
}


