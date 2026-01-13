using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Medical.API.Models.Entities
{
    /// <summary>
    /// 回答实体
    /// </summary>
    [Table("Answers")]
    public class Answer
    {
        [Key]
        public Guid Id { get; set; } = Guid.NewGuid();

        /// <summary>
        /// 问题ID
        /// </summary>
        [Required]
        public Guid QuestionId { get; set; }

        /// <summary>
        /// 患者ID（回答者）
        /// </summary>
        [Required]
        public Guid PatientId { get; set; }

        /// <summary>
        /// 内容
        /// </summary>
        [Required]
        [Column(TypeName = "text")]
        public string Content { get; set; } = string.Empty;

        /// <summary>
        /// 点赞数
        /// </summary>
        public int LikeCount { get; set; } = 0;

        /// <summary>
        /// 是否最佳答案
        /// </summary>
        public bool IsBestAnswer { get; set; } = false;

        /// <summary>
        /// 创建时间
        /// </summary>
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// 更新时间
        /// </summary>
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        // 导航属性
        [ForeignKey("QuestionId")]
        public virtual Question Question { get; set; } = null!;

        [ForeignKey("PatientId")]
        public virtual Patient Patient { get; set; } = null!;
    }
}
