using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Medical.API.Models.Entities
{
    /// <summary>
    /// 疾病分类实体
    /// </summary>
    [Table("DiseaseCategories")]
    public class DiseaseCategory
    {
        [Key]
        public Guid Id { get; set; } = Guid.NewGuid();

        /// <summary>
        /// 科室ID
        /// </summary>
        [Required]
        public Guid DepartmentId { get; set; }

        /// <summary>
        /// 疾病名称
        /// </summary>
        [Required]
        [MaxLength(100)]
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// 症状描述
        /// </summary>
        [MaxLength(500)]
        public string? Symptoms { get; set; }

        /// <summary>
        /// 创建时间
        /// </summary>
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // 导航属性
        [ForeignKey("DepartmentId")]
        public virtual Department Department { get; set; } = null!;
    }
}
