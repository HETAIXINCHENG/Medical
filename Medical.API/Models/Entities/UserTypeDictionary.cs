using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Medical.API.Models.Entities;

/// <summary>
/// 用户类型字典实体
/// </summary>
[Table("UserTypeDictionaries")]
public class UserTypeDictionary
{
    [Key]
    public int Id { get; set; }

    /// <summary>
    /// 类型名称：System、Doctor、Patient
    /// </summary>
    [Required]
    [MaxLength(50)]
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// 类型代码：1=System, 2=Doctor, 3=Patient
    /// </summary>
    [Required]
    public int Code { get; set; }

    /// <summary>
    /// 描述
    /// </summary>
    [MaxLength(200)]
    public string? Description { get; set; }

    /// <summary>
    /// 是否启用
    /// </summary>
    public bool IsActive { get; set; } = true;

    /// <summary>
    /// 排序
    /// </summary>
    public int SortOrder { get; set; } = 0;

    /// <summary>
    /// 创建时间
    /// </summary>
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// 更新时间
    /// </summary>
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    // 导航属性
    public virtual ICollection<User> Users { get; set; } = new List<User>();
}

