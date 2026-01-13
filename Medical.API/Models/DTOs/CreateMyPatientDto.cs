namespace Medical.API.Models.DTOs;

/// <summary>
/// 创建或更新当前用户患者信息的DTO
/// </summary>
public class CreateMyPatientDto
{
    /// <summary>
    /// 真实姓名
    /// </summary>
    public string RealName { get; set; } = string.Empty;

    /// <summary>
    /// 性别：Male（男）、Female（女）、Other（其他）
    /// </summary>
    public string? Gender { get; set; }

    /// <summary>
    /// 出生日期
    /// </summary>
    public DateTime? DateOfBirth { get; set; }

    /// <summary>
    /// 手机号（前端传入明文，后端加密）
    /// </summary>
    public string? PhoneNumber { get; set; }

    /// <summary>
    /// 邮箱（前端传入明文，后端加密）
    /// </summary>
    public string? Email { get; set; }

    /// <summary>
    /// 地址
    /// </summary>
    public string? Address { get; set; }
}

