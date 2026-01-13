using System.ComponentModel.DataAnnotations;

namespace Medical.API.Models.DTOs;

public class CreateDrugCategoryDto
{
    [Required(ErrorMessage = "分类名称不能为空")]
    [MaxLength(100, ErrorMessage = "分类名称长度不能超过100个字符")]
    public string CategoryName { get; set; } = string.Empty;

    public Guid? ParentId { get; set; }

    [MaxLength(500, ErrorMessage = "描述长度不能超过500个字符")]
    public string? Description { get; set; }

    public int SortOrder { get; set; } = 0;

    public bool IsActive { get; set; } = true;
}

