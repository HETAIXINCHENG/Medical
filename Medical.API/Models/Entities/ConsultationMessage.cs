using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace Medical.API.Models.Entities
{
    /// <summary>
    /// 咨询消息实体
    /// </summary>
    [Table("ConsultationMessages")]
    public class ConsultationMessage
    {
        [Key]
        public Guid Id { get; set; } = Guid.NewGuid();

        /// <summary>
        /// 咨询ID
        /// </summary>
        [Required]
        public Guid ConsultationId { get; set; }

        /// <summary>
        /// 发送者ID
        /// </summary>
        [Required]
        public Guid SenderId { get; set; }

        /// <summary>
        /// 是否为医生发送
        /// </summary>
        public bool IsFromDoctor { get; set; } = false;

        /// <summary>
        /// 消息内容（敏感信息加密存储）
        /// </summary>
        [Required]
        [MaxLength(2000)]
        public string Content { get; set; } = string.Empty;

        /// <summary>
        /// 消息类型：Text（文本）、Image（图片）、Voice（语音）、File（文件）
        /// </summary>
        [Required]
        [MaxLength(20)]
        public string MessageType { get; set; } = "Text";

        /// <summary>
        /// 附件URL
        /// </summary>
        [MaxLength(500)]
        public string? AttachmentUrl { get; set; }

        /// <summary>
        /// 是否已读
        /// </summary>
        public bool IsRead { get; set; } = false;

        /// <summary>
        /// 状态：Pending（待处理）、InProgress（进行中）、Completed（已完成）、Cancelled（已取消）
        /// </summary>
        [Required]
        [MaxLength(20)]
        public string Status { get; set; } = "Pending";

        /// <summary>
        /// 创建时间
        /// </summary>
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // 导航属性
        [ForeignKey("ConsultationId")]
        [JsonIgnore]
        public virtual Consultation Consultation { get; set; } = null!;
    }
}
