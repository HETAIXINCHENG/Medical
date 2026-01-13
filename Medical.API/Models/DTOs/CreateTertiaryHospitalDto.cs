using System.ComponentModel.DataAnnotations;

namespace Medical.API.Models.DTOs;

public class CreateTertiaryHospitalDto
{
    [Required(ErrorMessage = "医院名称不能为空")]
    [MaxLength(100, ErrorMessage = "医院名称长度不能超过100个字符")]
    public string Name { get; set; } = string.Empty;

    [Required(ErrorMessage = "省份ID不能为空")]
    public Guid ProvinceId { get; set; }

    [Required(ErrorMessage = "城市ID不能为空")]
    public Guid CityId { get; set; }

    [MaxLength(200, ErrorMessage = "地址长度不能超过200个字符")]
    public string? Address { get; set; }

    [MaxLength(20, ErrorMessage = "医院等级长度不能超过20个字符")]
    public string Level { get; set; } = "三甲";

    [MaxLength(50, ErrorMessage = "医院类型长度不能超过50个字符")]
    public string? Type { get; set; }

    [MaxLength(50, ErrorMessage = "联系电话长度不能超过50个字符")]
    public string? Phone { get; set; }

    [MaxLength(500, ErrorMessage = "官网长度不能超过500个字符")]
    [RegularExpression(@"^(https?:\/\/)?([\da-z\.-]+)\.([a-z\.]{2,6})([\/\w \.-]*)*\/?$", ErrorMessage = "官网格式不正确")]
    public string? Website { get; set; }

    public decimal? Latitude { get; set; }

    public decimal? Longitude { get; set; }

    public int SortOrder { get; set; } = 0;

    public bool IsEnabled { get; set; } = true;
}

