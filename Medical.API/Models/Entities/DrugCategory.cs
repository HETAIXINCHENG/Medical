using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace Medical.API.Models.Entities;

/// <summary>
/// 药品分类实体（支持无限层级分类）
/// </summary>
[Table("DrugCategories")]
public class DrugCategory
{
    [Key]
    public Guid Id { get; set; } = Guid.NewGuid();

    /// <summary>
    /// 分类名称（唯一）
    /// </summary>
    [Required]
    [MaxLength(100)]
    public string CategoryName { get; set; } = string.Empty;

    /// <summary>
    /// 父分类ID（自引用，支持无限层级）
    /// </summary>
    public Guid? ParentId { get; set; }

    /// <summary>
    /// 分类描述
    /// </summary>
    [MaxLength(500)]
    public string? Description { get; set; }

    /// <summary>
    /// 排序顺序
    /// </summary>
    public int SortOrder { get; set; } = 0;

    /// <summary>
    /// 是否启用
    /// </summary>
    public bool IsActive { get; set; } = true;

    /// <summary>
    /// 创建时间
    /// </summary>
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// 更新时间
    /// </summary>
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    // 导航属性
    [ForeignKey("ParentId")]
    [JsonIgnore]
    public virtual DrugCategory? Parent { get; set; }

    [JsonIgnore]
    public virtual ICollection<DrugCategory> Children { get; set; } = new List<DrugCategory>();

    [JsonIgnore]
    public virtual ICollection<Drug> Drugs { get; set; } = new List<Drug>();
}

