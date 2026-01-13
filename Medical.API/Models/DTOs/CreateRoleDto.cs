using System.ComponentModel.DataAnnotations;

namespace Medical.API.Models.DTOs;

public class CreateRoleDto
{
    [Required(ErrorMessage = "角色名称不能为空")]
    [MaxLength(50, ErrorMessage = "角色名称长度不能超过50个字符")]
    public string Name { get; set; } = string.Empty;

    [Required(ErrorMessage = "角色代码不能为空")]
    [MaxLength(50, ErrorMessage = "角色代码长度不能超过50个字符")]
    public string Code { get; set; } = string.Empty;

    [MaxLength(500, ErrorMessage = "描述长度不能超过500个字符")]
    public string? Description { get; set; }

    public bool IsActive { get; set; } = true;

    public int SortOrder { get; set; } = 0;
}

