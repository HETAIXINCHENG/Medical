using System.ComponentModel.DataAnnotations;

namespace Medical.API.Models.DTOs;

public class ResetPasswordDto
{
    [Required(ErrorMessage = "新密码不能为空")]
    [MinLength(6, ErrorMessage = "密码长度至少6个字符")]
    [MaxLength(50, ErrorMessage = "密码长度不能超过50个字符")]
    public string NewPassword { get; set; } = string.Empty;
}

