using Microsoft.EntityFrameworkCore;
using Medical.API.Models.Entities;

namespace Medical.API.Data;

/// <summary>
/// 医生统计数据初始化器（用于初始化现有数据的粉丝数和阅读总数）
/// </summary>
public static class DoctorStatisticsSeeder
{
    /// <summary>
    /// 初始化所有医生的统计数据（粉丝数和阅读总数）
    /// </summary>
    public static async Task SeedAsync(MedicalDbContext context)
    {
        var doctors = await context.Doctors.ToListAsync();
        
        foreach (var doctor in doctors)
        {
            // 统计订阅数（粉丝数）
            var followerCount = await context.UserDoctorSubscriptions
                .CountAsync(s => s.DoctorId == doctor.Id);
            
            // 统计该医生发布的健康知识总阅读量
            var totalReadCount = await context.HealthKnowledge
                .Where(k => !string.IsNullOrEmpty(k.Author) && k.Author == doctor.Id.ToString())
                .SumAsync(k => (int?)k.ReadCount) ?? 0;
            
            // 只在统计数据发生变化时才更新（避免不必要的数据库操作）
            // 注意：不更新 UpdatedAt，因为统计数据更新不应该触发 UpdatedAt 的更新
            if (doctor.FollowerCount != followerCount)
            {
                doctor.FollowerCount = followerCount;
            }
            if (doctor.TotalReadCount != totalReadCount)
            {
                doctor.TotalReadCount = totalReadCount;
            }
        }
        
        // EF Core 会自动检测变化，只在有实际变化时才执行 UPDATE
        await context.SaveChangesAsync();
    }
}

