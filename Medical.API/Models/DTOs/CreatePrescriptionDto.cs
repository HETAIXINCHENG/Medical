using System.ComponentModel.DataAnnotations;

namespace Medical.API.Models.DTOs;

public class CreatePrescriptionDto
{
    [Required(ErrorMessage = "患者ID不能为空")]
    public Guid PatientId { get; set; }

    [Required(ErrorMessage = "医生ID不能为空")]
    public Guid DoctorId { get; set; }

    public Guid? ConsultationId { get; set; }

    [Required(ErrorMessage = "处方编号不能为空")]
    [MaxLength(50, ErrorMessage = "处方编号长度不能超过50个字符")]
    public string PrescriptionNumber { get; set; } = string.Empty;

    [MaxLength(1000, ErrorMessage = "诊断结果长度不能超过1000个字符")]
    public string? Diagnosis { get; set; }

    [Required(ErrorMessage = "处方内容不能为空")]
    public string PrescriptionContent { get; set; } = string.Empty;

    [Required(ErrorMessage = "状态不能为空")]
    [MaxLength(20, ErrorMessage = "状态长度不能超过20个字符")]
    public string Status { get; set; } = "Draft";
}

