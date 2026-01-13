using System.ComponentModel.DataAnnotations;
using Medical.API.Models.Enums;

namespace Medical.API.Models.DTOs;

/// <summary>
/// 创建家庭成员DTO
/// </summary>
public class CreateFamilyMemberDto
{
    /// <summary>
    /// 姓名
    /// </summary>
    [Required(ErrorMessage = "姓名不能为空")]
    [MaxLength(50, ErrorMessage = "姓名长度不能超过50个字符")]
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// 关系类型：1=配偶, 2=子女, 3=父母, 4=其他
    /// </summary>
    [Required(ErrorMessage = "关系类型不能为空")]
    public int Relationship { get; set; }

    /// <summary>
    /// 性别：Male、Female
    /// </summary>
    [MaxLength(10, ErrorMessage = "性别长度不能超过10个字符")]
    public string? Gender { get; set; }

    /// <summary>
    /// 出生日期
    /// </summary>
    public DateTime? DateOfBirth { get; set; }

    /// <summary>
    /// 身份证号
    /// </summary>
    [MaxLength(50, ErrorMessage = "身份证号长度不能超过50个字符")]
    public string? IdCardNumber { get; set; }

    /// <summary>
    /// 手机号
    /// </summary>
    [MaxLength(50, ErrorMessage = "手机号长度不能超过50个字符")]
    public string? PhoneNumber { get; set; }

    /// <summary>
    /// 转换为 RelationshipType 枚举
    /// </summary>
    public RelationshipType GetRelationshipType()
    {
        if (Enum.IsDefined(typeof(RelationshipType), Relationship))
        {
            return (RelationshipType)Relationship;
        }
        return RelationshipType.Other;
    }
}

