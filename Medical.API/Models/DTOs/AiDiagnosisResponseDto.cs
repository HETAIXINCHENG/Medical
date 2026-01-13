namespace Medical.API.Models.DTOs;

/// <summary>
/// AI预诊响应DTO
/// </summary>
public class AiDiagnosisResponseDto
{
    /// <summary>
    /// 会话ID
    /// </summary>
    public string SessionId { get; set; } = string.Empty;

    /// <summary>
    /// 完成原因
    /// </summary>
    public string FinishReason { get; set; } = string.Empty;

    /// <summary>
    /// AI返回的文本内容
    /// </summary>
    public string Text { get; set; } = string.Empty;

    /// <summary>
    /// 使用情况统计
    /// </summary>
    public AiDiagnosisUsageDto? Usage { get; set; }

    /// <summary>
    /// 请求ID
    /// </summary>
    public string RequestId { get; set; } = string.Empty;
}

/// <summary>
/// AI预诊使用情况DTO
/// </summary>
public class AiDiagnosisUsageDto
{
    /// <summary>
    /// 模型使用情况列表
    /// </summary>
    public List<AiDiagnosisModelUsageDto> Models { get; set; } = new();
}

/// <summary>
/// AI预诊模型使用情况DTO
/// </summary>
public class AiDiagnosisModelUsageDto
{
    /// <summary>
    /// 模型ID
    /// </summary>
    public string ModelId { get; set; } = string.Empty;

    /// <summary>
    /// 输入token数
    /// </summary>
    public int InputTokens { get; set; }

    /// <summary>
    /// 输出token数
    /// </summary>
    public int OutputTokens { get; set; }
}

