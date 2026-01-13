using Medical.API.Models.Entities;
using Microsoft.EntityFrameworkCore;

namespace Medical.API.Data;

/// <summary>
/// 用户类型字典种子数据
/// </summary>
public static class UserTypeDictionarySeeder
{
    /// <summary>
    /// 初始化用户类型字典数据
    /// </summary>
    public static async Task SeedAsync(MedicalDbContext context)
    {
        // 检查是否已有数据
        if (await context.UserTypeDictionaries.AnyAsync())
        {
            return; // 已有数据，不重复插入
        }

        var userTypes = new[]
        {
            new UserTypeDictionary
            {
                Id = 1,
                Code = 1,
                Name = "System",
                Description = "系统用户",
                IsActive = true,
                SortOrder = 1,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            },
            new UserTypeDictionary
            {
                Id = 2,
                Code = 2,
                Name = "Doctor",
                Description = "医生",
                IsActive = true,
                SortOrder = 2,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            },
            new UserTypeDictionary
            {
                Id = 3,
                Code = 3,
                Name = "Patient",
                Description = "患者",
                IsActive = true,
                SortOrder = 3,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            }
        };

        context.UserTypeDictionaries.AddRange(userTypes);
        await context.SaveChangesAsync();
    }
}

