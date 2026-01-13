using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Medical.API.Models.Entities;

/// <summary>
/// 医生评价实体
/// </summary>
[Table("DoctorReviews")]
public class DoctorReview
{
    [Key]
    public Guid Id { get; set; } = Guid.NewGuid();

    /// <summary>
    /// 医生ID
    /// </summary>
    [Required]
    public Guid DoctorId { get; set; }

    /// <summary>
    /// 患者ID（评价者）
    /// </summary>
    [Required]
    public Guid PatientId { get; set; }

    /// <summary>
    /// 咨询ID（关联的咨询）
    /// </summary>
    public Guid? ConsultationId { get; set; }

    /// <summary>
    /// 评分（1-5星）
    /// </summary>
    [Required]
    [Range(1, 5)]
    public int Rating { get; set; } = 5;

    /// <summary>
    /// 评价内容
    /// </summary>
    [MaxLength(1000)]
    public string? Content { get; set; }

    /// <summary>
    /// 创建时间
    /// </summary>
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// 更新时间
    /// </summary>
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    // 导航属性
    [ForeignKey("DoctorId")]
    public virtual Doctor Doctor { get; set; } = null!;

    [ForeignKey("PatientId")]
    public virtual Patient Patient { get; set; } = null!;

    [ForeignKey("ConsultationId")]
    public virtual Consultation? Consultation { get; set; }
}

