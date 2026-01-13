using System.ComponentModel.DataAnnotations;

namespace Medical.API.Models.DTOs;

/// <summary>
/// 创建问题请求DTO
/// </summary>
public class CreateQuestionDto
{
    /// <summary>
    /// 患者ID（提问者）
    /// </summary>
    public Guid? PatientId { get; set; }

    /// <summary>
    /// 标题
    /// </summary>
    [Required(ErrorMessage = "标题不能为空")]
    [MaxLength(200, ErrorMessage = "标题最多200个字符")]
    public string Title { get; set; } = string.Empty;

    /// <summary>
    /// 内容
    /// </summary>
    [Required(ErrorMessage = "内容不能为空")]
    public string Content { get; set; } = string.Empty;

    /// <summary>
    /// 分类：热门、养生、问诊、保健、减肥、育儿类、失眠类等
    /// </summary>
    [MaxLength(50, ErrorMessage = "分类最多50个字符")]
    public string? Category { get; set; }

    /// <summary>
    /// 标签
    /// </summary>
    [MaxLength(200, ErrorMessage = "标签最多200个字符")]
    public string? Tags { get; set; }

    /// <summary>
    /// 收藏数
    /// </summary>
    public int FavoriteCount { get; set; } = 0;

    /// <summary>
    /// 回答数
    /// </summary>
    public int AnswerCount { get; set; } = 0;

    /// <summary>
    /// 查看数
    /// </summary>
    public int ViewCount { get; set; } = 0;

    /// <summary>
    /// 是否热门
    /// </summary>
    public bool IsHot { get; set; } = false;
}

