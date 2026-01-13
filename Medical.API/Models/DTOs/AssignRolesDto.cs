using System.ComponentModel.DataAnnotations;

namespace Medical.API.Models.DTOs;

public class AssignRolesDto
{
    [Required(ErrorMessage = "角色ID列表不能为空")]
    public List<Guid> RoleIds { get; set; } = new List<Guid>();
}

