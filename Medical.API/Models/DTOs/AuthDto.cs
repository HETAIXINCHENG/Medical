using System.ComponentModel.DataAnnotations;

namespace Medical.API.Models.DTOs;

/// <summary>
/// 登录请求DTO
/// </summary>
public class LoginRequestDto
{
    /// <summary>
    /// 用户名
    /// </summary>
    [Required(ErrorMessage = "用户名不能为空")]
    public string Username { get; set; } = string.Empty;

    /// <summary>
    /// 密码
    /// </summary>
    [Required(ErrorMessage = "密码不能为空")]
    public string Password { get; set; } = string.Empty;
}

/// <summary>
/// 注册请求DTO
/// </summary>
public class RegisterRequestDto
{
    /// <summary>
    /// 用户名
    /// </summary>
    [Required(ErrorMessage = "用户名不能为空")]
    [MinLength(3, ErrorMessage = "用户名至少3个字符")]
    [MaxLength(50, ErrorMessage = "用户名最多50个字符")]
    public string Username { get; set; } = string.Empty;

    /// <summary>
    /// 密码
    /// </summary>
    [Required(ErrorMessage = "密码不能为空")]
    [MinLength(6, ErrorMessage = "密码至少6个字符")]
    public string Password { get; set; } = string.Empty;

    /// <summary>
    /// 手机号
    /// </summary>
    [Phone(ErrorMessage = "手机号格式不正确")]
    public string? PhoneNumber { get; set; }

    /// <summary>
    /// 邮箱
    /// </summary>
    [EmailAddress(ErrorMessage = "邮箱格式不正确")]
    public string? Email { get; set; }

    /// <summary>
    /// 角色：Patient（患者）、Doctor（医生）
    /// </summary>
    public string Role { get; set; } = "Patient";
}

/// <summary>
/// 认证响应DTO
/// </summary>
public class AuthResponseDto
{
    /// <summary>
    /// Token
    /// </summary>
    public string Token { get; set; } = string.Empty;

    /// <summary>
    /// 用户信息
    /// </summary>
    public UserDto User { get; set; } = null!;
}

/// <summary>
/// 用户DTO
/// </summary>
public class UserDto
{
    public Guid Id { get; set; }
    public string Username { get; set; } = string.Empty;
    public string? PhoneNumber { get; set; }
    public string? Email { get; set; }
    public string? AvatarUrl { get; set; }
    public string? Bio { get; set; }
    /// <summary>
    /// 角色代码（用于绑定，如 "1", "2", "3"）
    /// </summary>
    public string Role { get; set; } = string.Empty;
    /// <summary>
    /// 角色名称（用于显示，如 "SuperAdmin", "Admin", "Business"）
    /// </summary>
    public string? RoleName { get; set; }
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; }
}

/// <summary>
/// 创建用户DTO（管理员）
/// </summary>
public class CreateUserDto
{
    [Required(ErrorMessage = "用户名不能为空")]
    public string Username { get; set; } = string.Empty;

    public string? Password { get; set; }

    [Phone(ErrorMessage = "手机号格式不正确")]
    public string? PhoneNumber { get; set; }

    [EmailAddress(ErrorMessage = "邮箱格式不正确")]
    public string? Email { get; set; }

    public string? Role { get; set; }

    public bool? IsActive { get; set; }
}

/// <summary>
/// 更新用户DTO（管理员）
/// </summary>
public class UpdateUserDto
{
    public string? Username { get; set; }

    public string? Password { get; set; }

    [Phone(ErrorMessage = "手机号格式不正确")]
    public string? PhoneNumber { get; set; }

    [EmailAddress(ErrorMessage = "邮箱格式不正确")]
    public string? Email { get; set; }

    public string? Role { get; set; }

    public bool? IsActive { get; set; }

    public string? AvatarUrl { get; set; }

    public string? Bio { get; set; }
}

/// <summary>
/// 管理员登录请求DTO（后台管理系统专用）
/// </summary>
public class AdminLoginRequestDto
{
    /// <summary>
    /// 用户名
    /// </summary>
    [Required(ErrorMessage = "用户名不能为空")]
    public string Username { get; set; } = string.Empty;

    /// <summary>
    /// 密码
    /// </summary>
    [Required(ErrorMessage = "密码不能为空")]
    public string Password { get; set; } = string.Empty;
}

/// <summary>
/// 修改密码请求DTO
/// </summary>
public class ChangePasswordRequestDto
{
    /// <summary>
    /// 原密码
    /// </summary>
    [Required(ErrorMessage = "原密码不能为空")]
    public string OldPassword { get; set; } = string.Empty;

    /// <summary>
    /// 新密码
    /// </summary>
    [Required(ErrorMessage = "新密码不能为空")]
    [MinLength(6, ErrorMessage = "新密码长度至少6个字符")]
    public string NewPassword { get; set; } = string.Empty;
}

