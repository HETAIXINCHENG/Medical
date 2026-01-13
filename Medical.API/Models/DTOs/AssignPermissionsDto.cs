using System.ComponentModel.DataAnnotations;

namespace Medical.API.Models.DTOs;

public class AssignPermissionsDto
{
    [Required(ErrorMessage = "权限ID列表不能为空")]
    public List<Guid> PermissionIds { get; set; } = new List<Guid>();
}

