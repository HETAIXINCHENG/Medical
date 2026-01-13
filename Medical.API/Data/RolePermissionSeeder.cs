using Medical.API.Models.Entities;
using Microsoft.EntityFrameworkCore;

namespace Medical.API.Data;

/// <summary>
/// 角色权限关联数据种子（为 SuperAdmin 和 Admin 分配所有权限）
/// </summary>
public static class RolePermissionSeeder
{
    /// <summary>
    /// 初始化角色权限关联数据
    /// </summary>
    public static async Task SeedAsync(MedicalDbContext context)
    {
        // 获取 SuperAdmin 和 Admin 角色
        var superAdminRole = await context.Roles
            .FirstOrDefaultAsync(r => r.Code == "1" || r.Name == "SuperAdmin");
        
        var adminRole = await context.Roles
            .FirstOrDefaultAsync(r => r.Code == "2" || r.Name == "Admin");

        if (superAdminRole == null || adminRole == null)
        {
            return; // 角色不存在，无法分配权限
        }

        // 获取所有权限
        var allPermissions = await context.Permissions.ToListAsync();

        if (allPermissions.Count == 0)
        {
            return; // 没有权限，无法分配
        }

        // 获取已存在的角色权限关联（用于增量添加）
        var existingRolePermissions = await context.RolePermissions
            .Where(rp => rp.RoleId == superAdminRole.Id || rp.RoleId == adminRole.Id)
            .Select(rp => new { rp.RoleId, rp.PermissionId })
            .ToListAsync();

        var rolePermissions = new List<RolePermission>();

        // 为 SuperAdmin 分配所有权限（增量添加，只添加不存在的）
        foreach (var permission in allPermissions)
        {
            var exists = existingRolePermissions.Any(erp => 
                erp.RoleId == superAdminRole.Id && erp.PermissionId == permission.Id);
            
            if (!exists)
            {
                rolePermissions.Add(new RolePermission
                {
                    Id = Guid.NewGuid(),
                    RoleId = superAdminRole.Id,
                    PermissionId = permission.Id,
                    CreatedAt = DateTime.UtcNow
                });
            }
        }

        // 为 Admin 分配所有权限（增量添加，只添加不存在的）
        foreach (var permission in allPermissions)
        {
            var exists = existingRolePermissions.Any(erp => 
                erp.RoleId == adminRole.Id && erp.PermissionId == permission.Id);
            
            if (!exists)
            {
                rolePermissions.Add(new RolePermission
                {
                    Id = Guid.NewGuid(),
                    RoleId = adminRole.Id,
                    PermissionId = permission.Id,
                    CreatedAt = DateTime.UtcNow
                });
            }
        }

        // 只有当有新权限需要分配时才执行保存
        if (rolePermissions.Count > 0)
        {
            context.RolePermissions.AddRange(rolePermissions);
            await context.SaveChangesAsync();
        }
    }
}

