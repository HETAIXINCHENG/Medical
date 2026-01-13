using System.ComponentModel.DataAnnotations;

namespace Medical.API.Models.DTOs;

/// <summary>
/// 创建咨询请求DTO
/// </summary>
public class CreateConsultationDto
{
    /// <summary>
    /// 患者ID
    /// </summary>
    public Guid? PatientId { get; set; }

    /// <summary>
    /// 医生ID
    /// </summary>
    [Required(ErrorMessage = "医生ID不能为空")]
    public Guid DoctorId { get; set; }

    /// <summary>
    /// 咨询类型：Text（图文）、Phone（电话）、Video（视频）、HomeVisit（上门）
    /// </summary>
    [Required(ErrorMessage = "咨询类型不能为空")]
    [MaxLength(20, ErrorMessage = "咨询类型最多20个字符")]
    public string ConsultationType { get; set; } = "Text";

    /// <summary>
    /// 价格（元）
    /// </summary>
    public decimal Price { get; set; } = 0;

    /// <summary>
    /// 开始时间
    /// </summary>
    public DateTime? StartTime { get; set; }

    /// <summary>
    /// 结束时间
    /// </summary>
    public DateTime? EndTime { get; set; }
}

