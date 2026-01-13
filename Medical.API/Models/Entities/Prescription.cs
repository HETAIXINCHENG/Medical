using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace Medical.API.Models.Entities;

/// <summary>
/// 处方实体
/// </summary>
[Table("Prescriptions")]
public class Prescription
{
    [Key]
    public Guid Id { get; set; } = Guid.NewGuid();

    /// <summary>
    /// 患者ID
    /// </summary>
    [Required]
    public Guid PatientId { get; set; }

    /// <summary>
    /// 医生ID
    /// </summary>
    [Required]
    public Guid DoctorId { get; set; }

    /// <summary>
    /// 咨询ID
    /// </summary>
    public Guid? ConsultationId { get; set; }

    /// <summary>
    /// 处方编号
    /// </summary>
    [Required]
    [MaxLength(50)]
    public string PrescriptionNumber { get; set; } = string.Empty;

    /// <summary>
    /// 诊断结果（加密存储）
    /// </summary>
    [MaxLength(1000)]
    public string? Diagnosis { get; set; }

    /// <summary>
    /// 处方内容（JSON格式，加密存储）
    /// </summary>
    [Required]
    [Column(TypeName = "text")]
    public string PrescriptionContent { get; set; } = string.Empty;

    /// <summary>
    /// 状态：Draft（草稿）、Issued（已开具）、Filled（已取药）、Cancelled（已取消）
    /// </summary>
    [Required]
    [MaxLength(20)]
    public string Status { get; set; } = "Draft";

    /// <summary>
    /// 创建时间
    /// </summary>
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// 更新时间
    /// </summary>
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    // 导航属性
    [ForeignKey("PatientId")]
    [JsonIgnore]
    public virtual Patient Patient { get; set; } = null!;

    [ForeignKey("DoctorId")]
    [JsonIgnore]
    public virtual Doctor Doctor { get; set; } = null!;

    [ForeignKey("ConsultationId")]
    [JsonIgnore]
    public virtual Consultation? Consultation { get; set; }

    [JsonIgnore]
    public virtual ICollection<PrescriptionMedicine> PrescriptionMedicines { get; set; } = new List<PrescriptionMedicine>();
}

/// <summary>
/// 处方药品关联实体
/// </summary>
[Table("PrescriptionMedicines")]
public class PrescriptionMedicine
{
    [Key]
    public Guid Id { get; set; } = Guid.NewGuid();

    /// <summary>
    /// 处方ID
    /// </summary>
    [Required]
    public Guid PrescriptionId { get; set; }

    /// <summary>
    /// 药品ID
    /// </summary>
    [Required]
    public Guid MedicineId { get; set; }

    /// <summary>
    /// 数量
    /// </summary>
    [Required]
    public int Quantity { get; set; } = 1;

    /// <summary>
    /// 用法用量
    /// </summary>
    [MaxLength(200)]
    public string? Usage { get; set; }

    // 导航属性
    [ForeignKey("PrescriptionId")]
    public virtual Prescription Prescription { get; set; } = null!;

    [ForeignKey("MedicineId")]
    public virtual Medicine Medicine { get; set; } = null!;
}

/// <summary>
/// 药品实体
/// </summary>
[Table("Medicines")]
public class Medicine
{
    [Key]
    public Guid Id { get; set; } = Guid.NewGuid();

    /// <summary>
    /// 药品名称
    /// </summary>
    [Required]
    [MaxLength(100)]
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// 药品规格
    /// </summary>
    [MaxLength(100)]
    public string? Specification { get; set; }

    /// <summary>
    /// 生产厂家
    /// </summary>
    [MaxLength(100)]
    public string? Manufacturer { get; set; }

    /// <summary>
    /// 价格
    /// </summary>
    [Column(TypeName = "decimal(10,2)")]
    public decimal Price { get; set; } = 0;

    /// <summary>
    /// 库存
    /// </summary>
    public int Stock { get; set; } = 0;

    /// <summary>
    /// 图片URL
    /// </summary>
    [MaxLength(500)]
    public string? ImageUrl { get; set; }

    /// <summary>
    /// 创建时间
    /// </summary>
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// 更新时间
    /// </summary>
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    // 导航属性
    public virtual ICollection<PrescriptionMedicine> PrescriptionMedicines { get; set; } = new List<PrescriptionMedicine>();
}

