using System.ComponentModel.DataAnnotations;

namespace Medical.API.Models.DTOs;

public class CreateConsultationMessageDto
{
    [Required(ErrorMessage = "咨询ID不能为空")]
    public Guid ConsultationId { get; set; }

    [Required(ErrorMessage = "消息内容不能为空")]
    [MaxLength(2000, ErrorMessage = "消息内容长度不能超过2000个字符")]
    public string Content { get; set; } = string.Empty;

    [Required(ErrorMessage = "消息类型不能为空")]
    [MaxLength(20, ErrorMessage = "消息类型长度不能超过20个字符")]
    public string MessageType { get; set; } = "Text"; // Text, Image, Voice, File, Video

    [MaxLength(500, ErrorMessage = "附件URL长度不能超过500个字符")]
    public string? AttachmentUrl { get; set; }

    public bool IsRead { get; set; } = false;

    /// <summary>
    /// 状态：Pending（待处理）、InProgress（进行中）、Completed（已完成）、Cancelled（已取消）
    /// </summary>
    [MaxLength(20, ErrorMessage = "状态最多20个字符")]
    public string? Status { get; set; }
}

