using System.ComponentModel.DataAnnotations;

namespace Medical.API.Models.DTOs;

public class CreatePatientDto
{
    [Required(ErrorMessage = "患者ID不能为空")]
    public Guid PatientId { get; set; }

    [Required(ErrorMessage = "真实姓名不能为空")]
    [MaxLength(50, ErrorMessage = "真实姓名长度不能超过50个字符")]
    public string RealName { get; set; } = string.Empty;

    [MaxLength(10, ErrorMessage = "性别长度不能超过10个字符")]
    public string? Gender { get; set; }

    public DateTime? DateOfBirth { get; set; }

    [MaxLength(50, ErrorMessage = "身份证号长度不能超过50个字符")]
    public string? IdCardNumber { get; set; }

    [MaxLength(20, ErrorMessage = "手机号长度不能超过20个字符")]
    public string? PhoneNumber { get; set; }

    [MaxLength(255, ErrorMessage = "邮箱长度不能超过255个字符")]
    public string? Email { get; set; }

    [MaxLength(500, ErrorMessage = "地址长度不能超过500个字符")]
    public string? Address { get; set; }

    // 紧急联系人信息（将保存到 FamilyMembers 表）
    [MaxLength(50, ErrorMessage = "紧急联系人姓名长度不能超过50个字符")]
    public string? EmergencyContactName { get; set; }

    [MaxLength(20, ErrorMessage = "紧急联系人电话长度不能超过20个字符")]
    public string? EmergencyContactPhone { get; set; }

    [MaxLength(20, ErrorMessage = "紧急联系人关系长度不能超过20个字符")]
    public string? EmergencyContactRelation { get; set; }

    [MaxLength(10, ErrorMessage = "紧急联系人性别长度不能超过10个字符")]
    public string? EmergencyContactGender { get; set; }

    public DateTime? EmergencyContactDateOfBirth { get; set; }

    [MaxLength(50, ErrorMessage = "紧急联系人身份证号长度不能超过50个字符")]
    public string? EmergencyContactIdCardNumber { get; set; }

    [MaxLength(10, ErrorMessage = "血型长度不能超过10个字符")]
    public string? BloodType { get; set; }

    public decimal? Height { get; set; }

    public decimal? Weight { get; set; }

    public string? AllergyHistory { get; set; }

    public string? MedicalHistory { get; set; }

    public string? FamilyHistory { get; set; }

    [MaxLength(1000, ErrorMessage = "备注长度不能超过1000个字符")]
    public string? Notes { get; set; }
}

