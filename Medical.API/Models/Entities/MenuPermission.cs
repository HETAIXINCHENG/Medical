using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace Medical.API.Models.Entities;

/// <summary>
/// 菜单权限实体（菜单项与权限的关联）
/// </summary>
[Table("MenuPermissions")]
public class MenuPermission
{
    [Key]
    public Guid Id { get; set; } = Guid.NewGuid();

    /// <summary>
    /// 菜单键（对应前端menuConfig中的key）
    /// </summary>
    [Required]
    [MaxLength(100)]
    public string MenuKey { get; set; } = string.Empty;

    /// <summary>
    /// 角色代码（关联到Roles表的Code字段）
    /// </summary>
    [Required]
    [MaxLength(50)]
    public string RoleCode { get; set; } = string.Empty;

    /// <summary>
    /// 菜单名称（用于显示）
    /// </summary>
    [MaxLength(100)]
    public string? MenuLabel { get; set; }

    /// <summary>
    /// 菜单路径（用于路由）
    /// </summary>
    [MaxLength(200)]
    public string? MenuPath { get; set; }

    /// <summary>
    /// 是否启用
    /// </summary>
    public bool IsActive { get; set; } = true;

    /// <summary>
    /// 排序顺序
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

    // 导航属性（可选，用于查询角色信息）
    // 注意：这里不使用外键约束，只存储 RoleCode 字符串
}

