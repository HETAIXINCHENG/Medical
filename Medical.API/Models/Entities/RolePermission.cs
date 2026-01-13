using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace Medical.API.Models.Entities;

/// <summary>
/// 角色-权限关联实体
/// </summary>
[Table("RolePermissions")]
public class RolePermission
{
    [Key]
    public Guid Id { get; set; } = Guid.NewGuid();

    /// <summary>
    /// 角色ID
    /// </summary>
    [Required]
    public Guid RoleId { get; set; }

    /// <summary>
    /// 权限ID
    /// </summary>
    [Required]
    public Guid PermissionId { get; set; }

    /// <summary>
    /// 创建时间
    /// </summary>
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // 导航属性
    [ForeignKey("RoleId")]
    [JsonIgnore]
    public virtual Role Role { get; set; } = null!;

    [ForeignKey("PermissionId")]
    [JsonIgnore]
    public virtual Permission Permission { get; set; } = null!;
}

