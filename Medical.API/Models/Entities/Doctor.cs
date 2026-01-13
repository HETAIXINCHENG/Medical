using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Medical.API.Models.Entities;

/// <summary>
/// 医生实体
/// </summary>
[Table("Doctors")]
public class Doctor
{
    [Key]
    public Guid Id { get; set; } = Guid.NewGuid();

    /// <summary>
    /// 科室ID
    /// </summary>
    [Required]
    public Guid DepartmentId { get; set; }

    /// <summary>
    /// 医生姓名
    /// </summary>
    [Required]
    [MaxLength(50)]
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// 职称：主治医师、副主任医师、主任医师等
    /// </summary>
    [Required]
    [MaxLength(50)]
    public string Title { get; set; } = string.Empty;

    /// <summary>
    /// 擅长领域
    /// </summary>
    [MaxLength(2000)]
    public string? Specialty { get; set; }

    /// <summary>
    /// 医院名称
    /// </summary>
    [MaxLength(100)]
    public string? Hospital { get; set; }

    /// <summary>
    /// 简介
    /// </summary>
    [MaxLength(6000)]
    public string? Introduction { get; set; }

    /// <summary>
    /// 头像URL
    /// </summary>
    [MaxLength(500)]
    public string? AvatarUrl { get; set; }

    /// <summary>
    /// 评分（平均分）
    /// </summary>
    [Column(TypeName = "decimal(3,2)")]
    public decimal Rating { get; set; } = 0;

    /// <summary>
    /// 咨询次数
    /// </summary>
    public int ConsultationCount { get; set; } = 0;

    /// <summary>
    /// 粉丝数（订阅数）
    /// </summary>
    public int FollowerCount { get; set; } = 0;

    /// <summary>
    /// 阅读总数（该医生发布的健康知识总阅读量）
    /// </summary>
    public int TotalReadCount { get; set; } = 0;

    /// <summary>
    /// 是否在线
    /// </summary>
    public bool IsOnline { get; set; } = false;

    /// <summary>
    /// 是否推荐
    /// </summary>
    public bool IsRecommended { get; set; } = false;

    /// <summary>
    /// 创建时间
    /// </summary>
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// 更新时间
    /// </summary>
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;


    /// <summary>
    /// 设定删除标志，假删除用户（永久封禁）
    /// </summary>
    public bool IsDeleted { get; set; } = false;

    // 导航属性
    [ForeignKey("DepartmentId")]
    public virtual Department Department { get; set; } = null!;

    public virtual ICollection<DoctorSchedule> Schedules { get; set; } = new List<DoctorSchedule>();
    public virtual ICollection<Consultation> Consultations { get; set; } = new List<Consultation>();
    public virtual ICollection<DoctorReview> Reviews { get; set; } = new List<DoctorReview>();
    
    // 多对多关系：医生-患者
    public virtual ICollection<DoctorPatient> DoctorPatients { get; set; } = new List<DoctorPatient>();
    
    // 患友会
    public virtual ICollection<PatientSupportGroup> PatientSupportGroups { get; set; } = new List<PatientSupportGroup>();
}

