using System.ComponentModel.DataAnnotations;

namespace Medical.API.Models.DTOs;

/// <summary>
/// 患友会DTO
/// </summary>
public class PatientSupportGroupDto
{
    public Guid Id { get; set; }
    public Guid DoctorId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public int PostCount { get; set; }
    public int TotalReadCount { get; set; }
    public bool IsEnabled { get; set; }
    public DateTime CreatedAt { get; set; }
    
    // 医生信息
    public string DoctorName { get; set; } = string.Empty;
    public string DoctorTitle { get; set; } = string.Empty;
    public string? DoctorAvatarUrl { get; set; }
    public string? DoctorHospital { get; set; }
    public string? DoctorDepartmentName { get; set; }
}

/// <summary>
/// 创建患友会DTO
/// </summary>
public class CreatePatientSupportGroupDto
{
    [Required(ErrorMessage = "医生ID不能为空")]
    public Guid DoctorId { get; set; }

    [MaxLength(100, ErrorMessage = "名称不能超过100个字符")]
    public string? Name { get; set; }

    [MaxLength(1000, ErrorMessage = "描述不能超过1000个字符")]
    public string? Description { get; set; }

    public bool IsEnabled { get; set; } = true;
}

/// <summary>
/// 更新患友会DTO
/// </summary>
public class UpdatePatientSupportGroupDto
{
    public Guid? DoctorId { get; set; }

    [MaxLength(100, ErrorMessage = "名称不能超过100个字符")]
    public string? Name { get; set; }

    [MaxLength(1000, ErrorMessage = "描述不能超过1000个字符")]
    public string? Description { get; set; }

    public bool? IsEnabled { get; set; }
}

/// <summary>
/// 帖子DTO
/// </summary>
public class PostDto
{
    public Guid Id { get; set; }
    public Guid PatientSupportGroupId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
    public string? Tag { get; set; }
    public int ReadCount { get; set; }
    public int CommentCount { get; set; }
    public int LikeCount { get; set; }
    public DateTime? LastReplyAt { get; set; }
    public bool IsPinned { get; set; }
    public DateTime CreatedAt { get; set; }
    
    // 发布者信息（脱敏显示）
    public string AuthorDisplayName { get; set; } = string.Empty;
    public string? AuthorAvatarUrl { get; set; }
    
    /// <summary>
    /// 附件URL列表
    /// </summary>
    public List<string>? AttachmentUrls { get; set; }
}

/// <summary>
/// 创建帖子DTO
/// </summary>
public class CreatePostDto
{
    public Guid PatientSupportGroupId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
    public string? Tag { get; set; }
    /// <summary>
    /// 附件URL列表（JSON数组格式）
    /// </summary>
    public List<string>? AttachmentUrls { get; set; }
}

/// <summary>
/// 评论DTO
/// </summary>
public class PostCommentDto
{
    public Guid Id { get; set; }
    public Guid PostId { get; set; }
    public string Content { get; set; } = string.Empty;
    public List<string>? AttachmentUrls { get; set; }
    public Guid? ParentCommentId { get; set; }
    public DateTime CreatedAt { get; set; }
    
    // 评论者信息（脱敏显示）
    public string AuthorDisplayName { get; set; } = string.Empty;
    public string? AuthorAvatarUrl { get; set; }
    
    // 回复列表
    public List<PostCommentDto>? Replies { get; set; }
}

/// <summary>
/// 创建评论DTO
/// </summary>
public class CreatePostCommentDto
{
    public Guid PostId { get; set; }
    public string Content { get; set; } = string.Empty;
    public Guid? ParentCommentId { get; set; }
    /// <summary>
    /// 附件URL列表（JSON数组格式）
    /// </summary>
    public List<string>? AttachmentUrls { get; set; }
}

