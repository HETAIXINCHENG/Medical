using System.ComponentModel.DataAnnotations;

namespace Medical.API.Models.DTOs;

/// <summary>
/// 更新病历DTO（用于追加补充）
/// </summary>
public class UpdateMedicalRecordDto
{
    /// <summary>
    /// 身高（cm）
    /// </summary>
    public decimal? Height { get; set; }

    /// <summary>
    /// 体重（kg）
    /// </summary>
    public decimal? Weight { get; set; }

    /// <summary>
    /// 本次患病时长描述
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
    /// 当前正在使用的药物
    /// </summary>
    [MaxLength(1000)]
    public string? CurrentMedications { get; set; }

    /// <summary>
    /// 当前是否怀孕
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
}

