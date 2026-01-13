using System.ComponentModel.DataAnnotations;

namespace Medical.API.Models.DTOs;

public class CreateExaminationReportDto
{
    [Required(ErrorMessage = "患者ID不能为空")]
    public Guid PatientId { get; set; }

    [Required(ErrorMessage = "医生ID不能为空")]
    public Guid DoctorId { get; set; }

    public Guid? VisitRecordId { get; set; }

    public Guid? ConsultationId { get; set; }

    [Required(ErrorMessage = "报告编号不能为空")]
    [MaxLength(50, ErrorMessage = "报告编号长度不能超过50个字符")]
    public string ReportNumber { get; set; } = string.Empty;

    [Required(ErrorMessage = "检查类型不能为空")]
    [MaxLength(50, ErrorMessage = "检查类型长度不能超过50个字符")]
    public string ExaminationType { get; set; } = string.Empty;

    [Required(ErrorMessage = "检查项目名称不能为空")]
    [MaxLength(200, ErrorMessage = "检查项目名称长度不能超过200个字符")]
    public string ExaminationName { get; set; } = string.Empty;

    [Required(ErrorMessage = "检查日期不能为空")]
    public DateTime ExaminationDate { get; set; } = DateTime.UtcNow;

    [Required(ErrorMessage = "报告日期不能为空")]
    public DateTime ReportDate { get; set; } = DateTime.UtcNow;

    [Required(ErrorMessage = "检查结果不能为空")]
    public string Results { get; set; } = string.Empty;

    [MaxLength(1000, ErrorMessage = "检查结论长度不能超过1000个字符")]
    public string? Conclusion { get; set; }

    public string? Recommendations { get; set; }

    [MaxLength(500, ErrorMessage = "报告文件URL长度不能超过500个字符")]
    public string? ReportFileUrl { get; set; }

    [Required(ErrorMessage = "状态不能为空")]
    [MaxLength(20, ErrorMessage = "状态长度不能超过20个字符")]
    public string Status { get; set; } = "Pending";

    [MaxLength(1000, ErrorMessage = "备注长度不能超过1000个字符")]
    public string? Notes { get; set; }
}

