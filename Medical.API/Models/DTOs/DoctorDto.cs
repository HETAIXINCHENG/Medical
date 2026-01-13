using System.ComponentModel.DataAnnotations;

namespace Medical.API.Models.DTOs;

/// <summary>
/// 创建医生请求DTO
/// </summary>
public class CreateDoctorDto
{
    [Required(ErrorMessage = "医生姓名不能为空")]
    [MaxLength(50)]
    public string Name { get; set; } = string.Empty;

    [Required(ErrorMessage = "职称不能为空")]
    [MaxLength(50)]
    public string Title { get; set; } = string.Empty;

    [MaxLength(100)]
    public string? Hospital { get; set; }

    [Required(ErrorMessage = "科室不能为空")]
    public Guid DepartmentId { get; set; }

    [MaxLength(500)]
    public string? Specialty { get; set; }

    [MaxLength(1000)]
    public string? Introduction { get; set; }

    [MaxLength(500)]
    public string? AvatarUrl { get; set; }

    public bool IsRecommended { get; set; } = false;
}

/// <summary>
/// 医生DTO
/// </summary>
public class DoctorDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string? Specialty { get; set; }
    public string? Hospital { get; set; }
    public string? Introduction { get; set; }
    public string? AvatarUrl { get; set; }
    public decimal Rating { get; set; }
    public int ConsultationCount { get; set; }
    public bool IsOnline { get; set; }
    public bool IsRecommended { get; set; }
    public Guid DepartmentId { get; set; }
    public string? DepartmentName { get; set; }
    /// <summary>
    /// 粉丝数（订阅数）
    /// </summary>
    public int FollowerCount { get; set; } = 0;
    /// <summary>
    /// 阅读总数（该医生发布的健康知识总阅读量）
    /// </summary>
    public int TotalReadCount { get; set; } = 0;
}

/// <summary>
/// 医生详情DTO
/// </summary>
public class DoctorDetailDto : DoctorDto
{
    public List<DoctorScheduleDto> Schedules { get; set; } = new();
    public List<DoctorReviewDto> RecentReviews { get; set; } = new();
    
    /// <summary>
    /// 电话咨询价格（元）
    /// </summary>
    public decimal? PhonePrice { get; set; }
    
    /// <summary>
    /// 图文咨询价格（元）
    /// </summary>
    public decimal? TextPrice { get; set; }
}

/// <summary>
/// 医生排班DTO
/// </summary>
public class DoctorScheduleDto
{
    public Guid Id { get; set; }
    public string DayOfWeek { get; set; } = string.Empty;
    public string TimeSlot { get; set; } = string.Empty;
    public bool IsAvailable { get; set; }
}

/// <summary>
/// 医生评价DTO
/// </summary>
public class DoctorReviewDto
{
    public Guid Id { get; set; }
    public int Rating { get; set; }
    public string? Content { get; set; }
    public string ReviewerName { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
}

