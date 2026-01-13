namespace Medical.API.Services;

/// <summary>
/// 权限验证服务接口
/// </summary>
public interface IPermissionService
{
    /// <summary>
    /// 检查用户是否拥有指定的权限代码
    /// </summary>
    /// <param name="userId">用户ID</param>
    /// <param name="permissionCode">权限代码</param>
    /// <returns>是否拥有权限</returns>
    Task<bool> HasPermissionAsync(Guid userId, string permissionCode);

    /// <summary>
    /// 获取用户的所有权限代码列表
    /// </summary>
    /// <param name="userId">用户ID</param>
    /// <returns>权限代码列表</returns>
    Task<List<string>> GetUserPermissionsAsync(Guid userId);
}

