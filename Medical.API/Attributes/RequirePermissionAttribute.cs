using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Medical.API.Services;
using Medical.API.Data;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using Microsoft.AspNetCore.Mvc.Authorization;
using Microsoft.AspNetCore.Mvc.Controllers;

namespace Medical.API.Attributes;

/// <summary>
/// 基于权限代码的授权特性
/// 用法: [RequirePermission("user.create")]
/// </summary>
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = true)]
public class RequirePermissionAttribute : AuthorizeAttribute, IAsyncAuthorizationFilter
{
    private readonly string _permissionCode;

    public RequirePermissionAttribute(string permissionCode)
    {
        _permissionCode = permissionCode;
    }

    public async Task OnAuthorizationAsync(AuthorizationFilterContext context)
    {
        // 检查是否有 AllowAnonymous 特性，如果有则允许匿名访问
        var endpoint = context.HttpContext.GetEndpoint();
        var allowAnonymous = endpoint?.Metadata?.GetMetadata<IAllowAnonymous>();
        
        // 也检查 ActionDescriptor 上的 AllowAnonymous
        if (allowAnonymous == null && context.ActionDescriptor is ControllerActionDescriptor actionDescriptor)
        {
            allowAnonymous = actionDescriptor.MethodInfo.GetCustomAttributes(typeof(AllowAnonymousAttribute), true)
                .FirstOrDefault() as IAllowAnonymous;
        }
        
        // 检查类级别是否有 AllowAnonymous
        if (allowAnonymous == null && context.ActionDescriptor is ControllerActionDescriptor controllerActionDescriptor)
        {
            allowAnonymous = controllerActionDescriptor.ControllerTypeInfo.GetCustomAttributes(typeof(AllowAnonymousAttribute), true)
                .FirstOrDefault() as IAllowAnonymous;
        }
        
        if (allowAnonymous != null)
        {
            return; // 允许匿名访问
        }

        // 如果用户未认证，让基础授权处理
        if (!context.HttpContext.User.Identity?.IsAuthenticated ?? true)
        {
            context.Result = new UnauthorizedResult(); // 返回 401 Unauthorized
            return;
        }

        // 获取用户ID
        var userIdClaim = context.HttpContext.User.FindFirst(ClaimTypes.NameIdentifier)?.Value
            ?? context.HttpContext.User.FindFirst("UserId")?.Value;

        if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out var userId))
        {
            context.Result = new ForbidResult();
            return;
        }

        // 从数据库查询用户的真实角色（通过 UserRoles 关联表）
        var dbContext = context.HttpContext.RequestServices.GetRequiredService<MedicalDbContext>();
        var userRoles = await dbContext.UserRoles
            .Where(ur => ur.UserId == userId)
            .Include(ur => ur.Role)
            .Select(ur => new { ur.Role.Code, ur.Role.Name })
            .ToListAsync();

        // 检查用户是否是 SuperAdmin 或 Admin，这些角色拥有所有权限
        var isSuperAdminOrAdmin = userRoles.Any(r => 
            r.Name == "SuperAdmin" || r.Name == "Admin" ||
            r.Code == "1" || r.Code == "2");

        if (isSuperAdminOrAdmin)
        {
            // SuperAdmin 和 Admin 拥有所有权限，直接通过
            return;
        }

        // 获取权限验证服务
        var permissionService = context.HttpContext.RequestServices.GetRequiredService<IPermissionService>();

        // 异步检查权限
        var hasPermission = await permissionService.HasPermissionAsync(userId, _permissionCode);

        if (!hasPermission)
        {
            context.Result = new ForbidResult();
        }
    }
}

