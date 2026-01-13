using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Medical.API.Models.Entities
{
    /// <summary>
    /// 医生排班实体
    /// </summary>
    [Table("DoctorSchedules")]
    public class DoctorSchedule
    {
        [Key]
        public Guid Id { get; set; } = Guid.NewGuid();

        /// <summary>
        /// 医生ID
        /// </summary>
        [Required]
        public Guid DoctorId { get; set; }

        /// <summary>
        /// 星期几：Monday, Tuesday, Wednesday, Thursday, Friday, Saturday, Sunday
        /// </summary>
        [Required]
        [MaxLength(20)]
        public string DayOfWeek { get; set; } = string.Empty;

        /// <summary>
        /// 时间段：Morning（上午）、Afternoon（下午）、Evening（晚上）
        /// </summary>
        [Required]
        [MaxLength(20)]
        public string TimeSlot { get; set; } = string.Empty;

        /// <summary>
        /// 是否可预约
        /// </summary>
        public bool IsAvailable { get; set; } = true;

        /// <summary>
        /// 最大预约数
        /// </summary>
        public int MaxAppointments { get; set; } = 10;

        /// <summary>
        /// 当前预约数
        /// </summary>
        public int CurrentAppointments { get; set; } = 0;

        /// <summary>
        /// 创建时间
        /// </summary>
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// 更新时间
        /// </summary>
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        // 导航属性
        [ForeignKey("DoctorId")]
        public virtual Doctor Doctor { get; set; } = null!;
    }
}
