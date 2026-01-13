using System.ComponentModel.DataAnnotations;

namespace Medical.API.Models.DTOs;

public class CreateDiseaseCategoryDto
{
    [Required(ErrorMessage = "科室ID不能为空")]
    public Guid DepartmentId { get; set; }

    [Required(ErrorMessage = "疾病名称不能为空")]
    [MaxLength(100, ErrorMessage = "疾病名称长度不能超过100个字符")]
    public string Name { get; set; } = string.Empty;

    [MaxLength(500, ErrorMessage = "症状描述长度不能超过500个字符")]
    public string? Symptoms { get; set; }
}

