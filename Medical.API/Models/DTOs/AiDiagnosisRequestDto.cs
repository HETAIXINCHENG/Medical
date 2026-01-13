using System.ComponentModel.DataAnnotations;
using static Medical.API.Controllers.AiDiagnosisController;

namespace Medical.API.Models.DTOs;

/// <summary>
/// AI预诊请求DTO
/// </summary>
public class AiDiagnosisRequestDto
{
    /// <summary>
    /// 用户输入的提示语/问题
    /// </summary>
    [Required(ErrorMessage = "提示语不能为空")]
    [MaxLength(2000, ErrorMessage = "提示语长度不能超过2000个字符")]
    public string Prompt { get; set; } = string.Empty;

    /// <summary>
    /// 图片 URL 列表，支持多图（目前一般一次传一张）
    /// </summary>
    public List<ImageInputDto>? Images { get; set; }
    /// <summary>
    /// 模型参数（可选）
    /// </summary>
    public AiDiagnosisParametersDto? Parameters { get; set; }
}
public class ImageInputDto
{
    /// <summary>
    /// 图片的完整 URL
    /// </summary>
    public string? Url { get; set; }
}

/// <summary>
/// AI预诊模型参数DTO
/// </summary>
public class AiDiagnosisParametersDto
{
    public float? Temperature { get; set; }
    public int? MaxTokens { get; set; }
}

