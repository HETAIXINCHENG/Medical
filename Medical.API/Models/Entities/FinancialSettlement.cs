using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Medical.API.Models.Entities;

/// <summary>
/// 财务结算批次
/// </summary>
[Table("FinancialSettlements")]
public class FinancialSettlement
{
    [Key]
    public Guid Id { get; set; } = Guid.NewGuid();

    /// <summary>
    /// 结算名称/批次名
    /// </summary>
    [Required]
    [MaxLength(100)]
    public string Name { get; set; } = string.Empty;

    public DateTime? PeriodStart { get; set; }
    public DateTime? PeriodEnd { get; set; }

    [Column(TypeName = "decimal(18,2)")]
    public decimal TotalReceivable { get; set; }

    [Column(TypeName = "decimal(18,2)")]
    public decimal TotalPayable { get; set; }

    [Column(TypeName = "decimal(18,2)")]
    public decimal NetAmount { get; set; }

    /// <summary>
    /// 状态（待结算/结算中/已完成/已关闭）
    /// </summary>
    [MaxLength(30)]
    public string Status { get; set; } = "pending";

    [MaxLength(500)]
    public string? Remark { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}

