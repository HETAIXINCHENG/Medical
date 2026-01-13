using Medical.API.Data;
using Microsoft.EntityFrameworkCore;

namespace Medical.API.Services;

/// <summary>
/// 权限验证服务实现
/// </summary>
public class PermissionService : IPermissionService
{
    private readonly MedicalDbContext _context;

    public PermissionService(MedicalDbContext context)
    {
        _context = context;
    }

    /// <summary>
    /// 检查用户是否拥有指定的权限代码
    /// </summary>
    public async Task<bool> HasPermissionAsync(Guid userId, string permissionCode)
    {
        // 查询用户 -> 用户角色 -> 角色权限 -> 权限代码
        var hasPermission = await _context.UserRoles
            .Where(ur => ur.UserId == userId)
            .SelectMany(ur => ur.Role.RolePermissions)
            .Where(rp => rp.Permission.Code == permissionCode && rp.Permission.IsActive)
            .AnyAsync();

        return hasPermission;
    }

    /// <summary>
    /// 获取用户的所有权限代码列表
    /// </summary>
    public async Task<List<string>> GetUserPermissionsAsync(Guid userId)
    {
        var permissionCodes = await _context.UserRoles
            .Where(ur => ur.UserId == userId)
            .SelectMany(ur => ur.Role.RolePermissions)
            .Where(rp => rp.Permission.IsActive)
            .Select(rp => rp.Permission.Code)
            .Distinct()
            .ToListAsync();

        return permissionCodes;
    }
}

