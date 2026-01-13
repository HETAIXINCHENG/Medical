using System.ComponentModel.DataAnnotations;

namespace Medical.API.Models.DTOs;

public class CreateVisitRecordDto
{
    [Required(ErrorMessage = "患者ID不能为空")]
    public Guid PatientId { get; set; }

    [Required(ErrorMessage = "医生ID不能为空")]
    public Guid DoctorId { get; set; }

    public Guid? ConsultationId { get; set; }

    [Required(ErrorMessage = "就诊日期不能为空")]
    public DateTime VisitDate { get; set; } = DateTime.UtcNow;

    [MaxLength(1000, ErrorMessage = "主诉长度不能超过1000个字符")]
    public string? ChiefComplaint { get; set; }

    public string? PresentIllness { get; set; }

    [MaxLength(1000, ErrorMessage = "诊断结果长度不能超过1000个字符")]
    public string? Diagnosis { get; set; }

    public string? TreatmentPlan { get; set; }

    public string? MedicalAdvice { get; set; }

    [Required(ErrorMessage = "就诊类型不能为空")]
    [MaxLength(20, ErrorMessage = "就诊类型长度不能超过20个字符")]
    public string VisitType { get; set; } = "Outpatient";

    [Required(ErrorMessage = "状态不能为空")]
    [MaxLength(20, ErrorMessage = "状态长度不能超过20个字符")]
    public string Status { get; set; } = "Completed";

    [MaxLength(1000, ErrorMessage = "备注长度不能超过1000个字符")]
    public string? Notes { get; set; }
}

