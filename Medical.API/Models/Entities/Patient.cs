using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace Medical.API.Models.Entities;

/// <summary>
/// 患者信息实体（与User表分开，存储患者详细信息和过敏史）
/// </summary>
[Table("Patients")]
public class Patient
{
    [Key]
    public Guid Id { get; set; } = Guid.NewGuid();

    /// <summary>
    /// 关联的用户ID（User表的外键）
    /// </summary>
    [Required]
    public Guid UserId { get; set; }

    /// <summary>
    /// 真实姓名
    /// </summary>
    [Required]
    [MaxLength(50)]
    public string RealName { get; set; } = string.Empty;

    /// <summary>
    /// 性别：Male（男）、Female（女）、Other（其他）
    /// </summary>
    [MaxLength(10)]
    public string? Gender { get; set; }

    /// <summary>
    /// 出生日期
    /// </summary>
    public DateTime? DateOfBirth { get; set; }

    /// <summary>
    /// 身份证号
    /// </summary>
    [MaxLength(50)]
    public string? IdCardNumber { get; set; }

    /// <summary>
    /// 手机号
    /// </summary>
    [MaxLength(50)]
    public string? PhoneNumber { get; set; }

    /// <summary>
    /// 邮箱
    /// </summary>
    [MaxLength(50)]
    public string? Email { get; set; }

    /// <summary>
    /// 地址
    /// </summary>
    [MaxLength(500)]
    public string? Address { get; set; }

    /// <summary>
    /// 血型：A、B、AB、O、Unknown
    /// </summary>
    [MaxLength(10)]
    public string? BloodType { get; set; }

    /// <summary>
    /// 身高（cm）
    /// </summary>
    public decimal? Height { get; set; }

    /// <summary>
    /// 体重（kg）
    /// </summary>
    public decimal? Weight { get; set; }

    /// <summary>
    /// BMI（身体质量指数，自动计算：体重(kg) / 身高(m)²）
    /// </summary>
    public decimal? BMI { get; set; }

    /// <summary>
    /// 过敏史（JSON格式存储，包含过敏原、症状、严重程度等）
    /// </summary>
    [Column(TypeName = "text")]
    public string? AllergyHistory { get; set; }

    /// <summary>
    /// 既往病史（JSON格式存储）
    /// </summary>
    [Column(TypeName = "text")]
    public string? MedicalHistory { get; set; }

    /// <summary>
    /// 家族病史（JSON格式存储）
    /// </summary>
    [Column(TypeName = "text")]
    public string? FamilyHistory { get; set; }

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
    [ForeignKey("UserId")]
    [JsonIgnore]
    public virtual User User { get; set; } = null!;

    /// <summary>
    /// 家庭成员列表
    /// </summary>
    [JsonIgnore]
    public virtual ICollection<FamilyMember> FamilyMembers { get; set; } = new List<FamilyMember>();

    /// <summary>
    /// 医生评价列表
    /// </summary>
    [JsonIgnore]
    public virtual ICollection<DoctorReview> DoctorReviews { get; set; } = new List<DoctorReview>();

    /// <summary>
    /// 处方列表（一对多关系）
    /// </summary>
    [JsonIgnore]
    public virtual ICollection<Prescription> Prescriptions { get; set; } = new List<Prescription>();

    /// <summary>
    /// 就诊记录列表（一对多关系）
    /// </summary>
    [JsonIgnore]
    public virtual ICollection<VisitRecord> VisitRecords { get; set; } = new List<VisitRecord>();

    /// <summary>
    /// 检查报告列表（一对多关系）
    /// </summary>
    [JsonIgnore]
    public virtual ICollection<ExaminationReport> ExaminationReports { get; set; } = new List<ExaminationReport>();

    // 多对多关系
    /// <summary>
    /// 医生-患者关联（多对多）
    /// </summary>
    [JsonIgnore]
    public virtual ICollection<DoctorPatient> DoctorPatients { get; set; } = new List<DoctorPatient>();

    // 注意：Consultation 和 Patient 的关系已改为多对多，通过 ConsultationPatient 关联表实现
    // 不再使用一对多关系，因此移除了 Consultations 导航属性

    /// <summary>
    /// 问题列表（一对多关系）
    /// </summary>
    [JsonIgnore]
    public virtual ICollection<Question> Questions { get; set; } = new List<Question>();

    /// <summary>
    /// 回答列表（一对多关系）
    /// </summary>
    [JsonIgnore]
    public virtual ICollection<Answer> Answers { get; set; } = new List<Answer>();

}

