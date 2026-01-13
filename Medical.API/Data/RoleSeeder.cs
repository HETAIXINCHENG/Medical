using Medical.API.Models.Entities;
using Microsoft.EntityFrameworkCore;

namespace Medical.API.Data;

/// <summary>
/// 角色种子数据
/// </summary>
public static class RoleSeeder
{
    /// <summary>
    /// 初始化角色数据
    /// </summary>
    public static async Task SeedAsync(MedicalDbContext context)
    {
        // 检查是否已有数据
        if (await context.Roles.AnyAsync())
        {
            return; // 已有数据，不重复插入
        }

        var roles = new[]
        {
            new Role
            {
                Name = "SuperAdmin",
                Code = "1",
                Description = "超级管理员",
                IsActive = true,
                SortOrder = 1,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            },
            new Role
            {
                Name = "Admin",
                Code = "2",
                Description = "管理员",
                IsActive = true,
                SortOrder = 2,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            },
            new Role
            {
                Name = "Business",
                Code = "3",
                Description = "业务员",
                IsActive = true,
                SortOrder = 3,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            }
        };

        context.Roles.AddRange(roles);
        await context.SaveChangesAsync();
    }
}

