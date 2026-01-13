using Medical.API.Models.Entities;
using Medical.API.Services;
using Microsoft.EntityFrameworkCore;

namespace Medical.API.Data;

/// <summary>
/// 默认管理员用户种子数据
/// </summary>
public static class DefaultAdminSeeder
{
    /// <summary>
    /// 初始化默认管理员用户（admin/123456）
    /// </summary>
    public static async Task SeedAsync(MedicalDbContext context, IEncryptionService encryptionService)
    {
        // 检查是否已存在 admin 用户
        var existingAdmin = await context.Users
            .Include(u => u.UserRoles)
            .FirstOrDefaultAsync(u => u.Username == "admin");

        if (existingAdmin != null)
        {
            // 如果已存在，确保其配置正确
            existingAdmin.UserTypeId = 1; // System
            existingAdmin.IsActive = true;
            existingAdmin.PasswordHash = encryptionService.HashPassword("123456"); // 重置密码为 123456
            existingAdmin.UpdatedAt = DateTime.UtcNow;

            // 确保 admin 用户有 SuperAdmin 角色
            var superAdminRole = await context.Roles
                .FirstOrDefaultAsync(r => r.Code == "1" || r.Name == "SuperAdmin");

            if (superAdminRole != null)
            {
                // 更新 Role 字段为 Role Name
                existingAdmin.Role = superAdminRole.Name;

                // 检查是否已有 UserRole 关联
                var existingUserRole = await context.UserRoles
                    .FirstOrDefaultAsync(ur => ur.UserId == existingAdmin.Id && ur.RoleId == superAdminRole.Id);

                if (existingUserRole == null)
                {
                    // 创建 UserRole 关联
                    context.UserRoles.Add(new UserRole
                    {
                        UserId = existingAdmin.Id,
                        RoleId = superAdminRole.Id,
                        CreatedAt = DateTime.UtcNow
                    });
                }
            }

            await context.SaveChangesAsync();
            return;
        }

        // 获取 SuperAdmin 角色
        var superAdminRoleForNew = await context.Roles
            .FirstOrDefaultAsync(r => r.Code == "1" || r.Name == "SuperAdmin");

        if (superAdminRoleForNew == null)
        {
            throw new InvalidOperationException("SuperAdmin 角色不存在，请先初始化角色数据");
        }

        // 创建新的 admin 用户
        var adminUser = new User
        {
            Username = "admin",
            PasswordHash = encryptionService.HashPassword("123456"),
            UserTypeId = 1, // System
            Role = superAdminRoleForNew.Name, // 存储 Role Name
            IsActive = true,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        context.Users.Add(adminUser);
        await context.SaveChangesAsync(); // 先保存以获取 Id

        // 创建 UserRole 关联
        context.UserRoles.Add(new UserRole
        {
            UserId = adminUser.Id,
            RoleId = superAdminRoleForNew.Id,
            CreatedAt = DateTime.UtcNow
        });

        await context.SaveChangesAsync();
    }
}

