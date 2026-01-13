using System.ComponentModel.DataAnnotations;

namespace Medical.API.Models.DTOs;

public class CreateDrugDto
{
    [Required(ErrorMessage = "药品通用名不能为空")]
    [MaxLength(100, ErrorMessage = "药品通用名长度不能超过100个字符")]
    public string CommonName { get; set; } = string.Empty;

    [MaxLength(100, ErrorMessage = "药品商品名长度不能超过100个字符")]
    public string? TradeName { get; set; }

    [Required(ErrorMessage = "药品规格不能为空")]
    [MaxLength(50, ErrorMessage = "药品规格长度不能超过50个字符")]
    public string Specification { get; set; } = string.Empty;

    [Required(ErrorMessage = "生产厂家不能为空")]
    [MaxLength(100, ErrorMessage = "生产厂家长度不能超过100个字符")]
    public string Manufacturer { get; set; } = string.Empty;

    [Required(ErrorMessage = "国药准字批准文号不能为空")]
    [MaxLength(50, ErrorMessage = "批准文号长度不能超过50个字符")]
    public string ApprovalNumber { get; set; } = string.Empty;

    [Required(ErrorMessage = "所属分类不能为空")]
    public Guid CategoryId { get; set; }

    [Required(ErrorMessage = "药品单位不能为空")]
    [MaxLength(20, ErrorMessage = "药品单位长度不能超过20个字符")]
    public string Unit { get; set; } = string.Empty;

    [MaxLength(50, ErrorMessage = "储存条件长度不能超过50个字符")]
    public string StorageCondition { get; set; } = "常温";

    public bool IsActive { get; set; } = true;
}

