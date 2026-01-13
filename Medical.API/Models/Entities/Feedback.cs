using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Medical.API.Models.Entities
{
    /// <summary>
    /// 系统反馈表
    /// </summary>
    [Table("Feedbacks")]
    public class Feedback
    {
        [Key]
        public Guid Id { get; set; } = Guid.NewGuid();

        /// <summary>
        /// 反馈标题
        /// </summary>
        [Required]
        [MaxLength(100)]
        public string Title { get; set; } = string.Empty;

        /// <summary>
        /// 反馈内容
        /// </summary>
        [Required]
        [MaxLength(2000)]
        [Column(TypeName = "text")]
        public string Content { get; set; } = string.Empty;

        /// <summary>
        /// 处理状态（Pending/Processing/Resolved/Closed）
        /// </summary>
        [Required]
        [MaxLength(50)]
        public string Status { get; set; } = "Pending";

        /// <summary>
        /// 处理备注
        /// </summary>
        [Column(TypeName = "text")]
        public string? ProcessNote { get; set; }

        /// <summary>
        /// 处理人ID（管理员）
        /// </summary>
        public Guid? ProcessedBy { get; set; }

        /// <summary>
        /// 处理时间
        /// </summary>
        public DateTime? ProcessedAt { get; set; }

        /// <summary>
        /// 创建时间
        /// </summary>
        [Required]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// 更新时间
        /// </summary>
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    }
}

