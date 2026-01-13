# 权限代码授权使用说明

## 概述

`RequirePermissionAttribute` 是一个自定义的授权特性，用于基于权限代码（Permission Code）进行细粒度的权限控制。

## 工作原理

1. **权限代码**：每个权限都有一个唯一的代码（如 `user.create`、`user.update`、`user.delete`）
2. **用户角色**：用户通过 `UserRoles` 关联到角色（Role）
3. **角色权限**：角色通过 `RolePermissions` 关联到权限（Permission）
4. **权限验证**：当用户访问受保护的方法时，系统会检查用户是否拥有对应的权限代码

## 使用方法

### 1. 在 Controller 方法上使用

```csharp
using Medical.API.Attributes;

[ApiController]
[Route("api/[controller]")]
public class UsersController : ControllerBase
{
    // 使用权限代码控制访问
    [HttpGet]
    [RequirePermission("user.view")]
    public async Task<ActionResult> GetAllUsers()
    {
        // 只有拥有 "user.view" 权限的用户才能访问
    }

    [HttpPost]
    [RequirePermission("user.create")]
    public async Task<ActionResult> CreateUser([FromBody] CreateUserDto dto)
    {
        // 只有拥有 "user.create" 权限的用户才能访问
    }

    [HttpPut("{id}")]
    [RequirePermission("user.update")]
    public async Task<ActionResult> UpdateUser(Guid id, [FromBody] UpdateUserDto dto)
    {
        // 只有拥有 "user.update" 权限的用户才能访问
    }

    [HttpDelete("{id}")]
    [RequirePermission("user.delete")]
    public async Task<ActionResult> DeleteUser(Guid id)
    {
        // 只有拥有 "user.delete" 权限的用户才能访问
    }
}
```

### 2. 在 Controller 类上使用（应用于所有方法）

```csharp
[ApiController]
[Route("api/[controller]")]
[RequirePermission("permission.manage")] // 整个 Controller 都需要此权限
public class PermissionsController : ControllerBase
{
    // 所有方法都需要 "permission.manage" 权限
}
```

### 3. 与角色授权结合使用

```csharp
[HttpGet]
[Authorize(Roles = "Admin,SuperAdmin")] // 先检查角色
[RequirePermission("user.view")]        // 再检查权限代码
public async Task<ActionResult> GetAllUsers()
{
    // 用户必须是 Admin 或 SuperAdmin 角色，并且拥有 "user.view" 权限
}
```

## 权限代码命名规范

建议使用以下格式：`资源.操作`

- `user.create` - 创建用户
- `user.update` - 更新用户
- `user.delete` - 删除用户
- `user.view` - 查看用户
- `permission.manage` - 管理权限
- `role.manage` - 管理角色

## 权限验证流程

```
用户请求 → JWT 认证 → 获取用户ID → 查询用户角色 → 查询角色权限 → 检查权限代码 → 允许/拒绝访问
```

## 注意事项

1. **必须先认证**：`RequirePermission` 特性要求用户已经通过 JWT 认证
2. **权限代码必须存在**：确保数据库中已经创建了对应的权限记录
3. **角色必须分配权限**：确保角色已经通过 `RolePermissions` 表分配了相应的权限
4. **权限必须激活**：只有 `IsActive = true` 的权限才会被验证

## 错误响应

如果用户没有权限，系统会返回 `403 Forbidden` 状态码。

