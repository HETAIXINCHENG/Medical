using System.ComponentModel.DataAnnotations;

namespace Medical.API.Models.DTOs
{
    public class FeedbackDto
    {
        public Guid Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Content { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public string? ProcessNote { get; set; }
        public Guid? ProcessedBy { get; set; }
        public string? ProcessorName { get; set; }
        public DateTime? ProcessedAt { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
    }

    public class CreateFeedbackDto
    {
        [Required(ErrorMessage = "标题不能为空")]
        [MaxLength(100, ErrorMessage = "标题不能超过100个字符")]
        public string Title { get; set; } = string.Empty;

        [Required(ErrorMessage = "反馈内容不能为空")]
        [MaxLength(2000, ErrorMessage = "反馈内容不能超过2000个字符")]
        public string Content { get; set; } = string.Empty;
    }

    public class UpdateFeedbackDto
    {
        [MaxLength(100, ErrorMessage = "标题不能超过100个字符")]
        public string? Title { get; set; }

        [MaxLength(2000, ErrorMessage = "反馈内容不能超过2000个字符")]
        public string? Content { get; set; }

        [MaxLength(50)]
        public string? Status { get; set; }

        [MaxLength(1000, ErrorMessage = "处理备注不能超过1000个字符")]
        public string? ProcessNote { get; set; }
    }
}

