using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace Medical.API.Models.Entities;

/// <summary>
/// 用户-角色关联实体
/// </summary>
[Table("UserRoles")]
public class UserRole
{
    [Key]
    public Guid Id { get; set; } = Guid.NewGuid();

    /// <summary>
    /// 用户ID
    /// </summary>
    [Required]
    public Guid UserId { get; set; }

    /// <summary>
    /// 角色ID
    /// </summary>
    [Required]
    public Guid RoleId { get; set; }

    /// <summary>
    /// 创建时间
    /// </summary>
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // 导航属性
    [ForeignKey("UserId")]
    [JsonIgnore]
    public virtual User User { get; set; } = null!;

    [ForeignKey("RoleId")]
    [JsonIgnore]
    public virtual Role Role { get; set; } = null!;
}

