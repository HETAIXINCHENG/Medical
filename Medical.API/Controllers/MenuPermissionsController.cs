using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using Medical.API.Data;
using Medical.API.Models.Entities;
using Medical.API.Attributes;
using System.Security.Claims;

namespace Medical.API.Controllers;

/// <summary>
/// 菜单权限配置控制器
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
[Authorize(Roles = "Admin,SuperAdmin")]
public class MenuPermissionsController : ControllerBase
{
    private readonly MedicalDbContext _context;
    private readonly ILogger<MenuPermissionsController> _logger;

    public MenuPermissionsController(MedicalDbContext context, ILogger<MenuPermissionsController> logger)
    {
        _context = context;
        _logger = logger;
    }

    /// <summary>
    /// 获取所有菜单权限配置
    /// </summary>
    /// <returns>菜单权限配置列表</returns>
    [HttpGet]
    [RequirePermission("menupermission.view")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<ActionResult> GetMenuPermissions()
    {
        var menuPermissions = await _context.MenuPermissions
            .OrderBy(mp => mp.SortOrder)
            .ThenBy(mp => mp.MenuKey)
            .Select(mp => new
            {
                mp.Id,
                mp.MenuKey,
                mp.MenuLabel,
                mp.MenuPath,
                mp.RoleCode,
                mp.IsActive,
                mp.SortOrder,
                mp.CreatedAt,
                mp.UpdatedAt
            })
            .ToListAsync();

        // 获取所有角色信息用于显示
        var roles = await _context.Roles
            .Select(r => new { r.Code, r.Name })
            .ToListAsync();

        var roleMap = roles.ToDictionary(r => r.Code, r => r.Name);

        // 为每个菜单权限添加角色名称
        var result = menuPermissions.Select(mp => new
        {
            mp.Id,
            mp.MenuKey,
            mp.MenuLabel,
            mp.MenuPath,
            mp.RoleCode,
            RoleName = roleMap.ContainsKey(mp.RoleCode) ? roleMap[mp.RoleCode] : mp.RoleCode,
            mp.IsActive,
            mp.SortOrder,
            mp.CreatedAt,
            mp.UpdatedAt
        }).ToList();

        return Ok(result);
    }

    /// <summary>
    /// 创建或更新菜单权限配置
    /// </summary>
    /// <param name="dto">菜单权限配置</param>
    /// <returns>创建或更新的配置</returns>
    [HttpPost]
    [RequirePermission("menupermission.create")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult> CreateOrUpdateMenuPermission([FromBody] CreateMenuPermissionDto dto)
    {
        try
        {
            if (dto == null || string.IsNullOrWhiteSpace(dto.MenuKey))
            {
                return BadRequest(new { message = "菜单键不能为空" });
            }

            if (string.IsNullOrWhiteSpace(dto.RoleCode))
            {
                return BadRequest(new { message = "角色代码不能为空" });
            }

            // 检查角色是否存在
            var role = await _context.Roles.FirstOrDefaultAsync(r => r.Code == dto.RoleCode);
            if (role == null)
            {
                return BadRequest(new { message = "角色不存在" });
            }

            // 检查菜单键和角色代码的组合是否已存在
            var existing = await _context.MenuPermissions
                .FirstOrDefaultAsync(mp => mp.MenuKey == dto.MenuKey && mp.RoleCode == dto.RoleCode);

            if (existing != null)
            {
                // 更新现有配置
                existing.MenuLabel = dto.MenuLabel;
                existing.MenuPath = dto.MenuPath;
                existing.IsActive = dto.IsActive;
                existing.SortOrder = dto.SortOrder;
                existing.UpdatedAt = DateTime.UtcNow;

                await _context.SaveChangesAsync();
                _logger.LogInformation("菜单权限配置更新成功: MenuKey={MenuKey}, RoleCode={RoleCode}", dto.MenuKey, dto.RoleCode);
                return Ok(existing);
            }
            else
            {
                // 创建新配置
                var newMenuPermission = new MenuPermission
                {
                    Id = Guid.NewGuid(),
                    MenuKey = dto.MenuKey,
                    RoleCode = dto.RoleCode,
                    MenuLabel = dto.MenuLabel,
                    MenuPath = dto.MenuPath,
                    IsActive = dto.IsActive,
                    SortOrder = dto.SortOrder,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };

                _context.MenuPermissions.Add(newMenuPermission);
                await _context.SaveChangesAsync();
                _logger.LogInformation("菜单权限配置创建成功: MenuKey={MenuKey}, RoleCode={RoleCode}", dto.MenuKey, dto.RoleCode);
                return Ok(newMenuPermission);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "创建或更新菜单权限配置失败");
            return BadRequest(new { message = "操作失败", error = ex.Message });
        }
    }

    /// <summary>
    /// 删除菜单权限配置
    /// </summary>
    /// <param name="id">配置ID</param>
    /// <returns>删除结果</returns>
    [HttpDelete("{id}")]
    [RequirePermission("menupermission.delete")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteMenuPermission(Guid id)
    {
        var menuPermission = await _context.MenuPermissions.FindAsync(id);
        if (menuPermission == null)
        {
            return NotFound(new { message = "菜单权限配置不存在" });
        }

        _context.MenuPermissions.Remove(menuPermission);
        await _context.SaveChangesAsync();

        _logger.LogInformation("菜单权限配置删除成功: Id={Id}", id);
        return NoContent();
    }

    /// <summary>
    /// 获取当前用户有权限访问的菜单列表
    /// Admin 和 SuperAdmin 角色默认显示所有菜单
    /// </summary>
    /// <returns>菜单列表</returns>
    [HttpGet("my-menus")]
    [Authorize]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<ActionResult> GetMyMenus()
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        var userRole = User.FindFirstValue(ClaimTypes.Role);
        
        if (string.IsNullOrEmpty(userId) || !Guid.TryParse(userId, out var userIdGuid))
        {
            return Unauthorized();
        }

        // 查询用户的角色（通过 UserRoles 关联表）
        var userRolesList = await _context.UserRoles
            .Where(ur => ur.UserId == userIdGuid)
            .Include(ur => ur.Role)
            .Select(ur => ur.Role)
            .ToListAsync();

        // 检查用户是否有 Admin 或 SuperAdmin 角色
        // 根据 roles 表：SuperAdmin Code=1, Admin Code=2
        bool isAdmin = userRolesList.Any(r => 
            r.Code == "1" || r.Code == "2" || 
            r.Name == "SuperAdmin" || r.Name == "Admin");

        // 也检查 Token 中的角色（兼容）
        bool isAdminFromToken = userRole == "Admin" || userRole == "SuperAdmin" ||
                               userRole == "1" || userRole == "2";

        // Admin 和 SuperAdmin 角色默认显示所有菜单
        if (isAdmin || isAdminFromToken)
        {
            // 返回 null 表示显示所有菜单
            return Ok(null);
        }

        // 获取用户角色的代码列表
        var userRoleCodes = userRolesList.Select(r => r.Code).ToList();

        if (userRoleCodes.Count == 0)
        {
            return Ok(new List<object>());
        }

        // 获取这些角色代码对应的菜单
        var menus = await _context.MenuPermissions
            .Where(mp => mp.IsActive && userRoleCodes.Contains(mp.RoleCode))
            .OrderBy(mp => mp.SortOrder)
            .ThenBy(mp => mp.MenuKey)
            .Select(mp => new
            {
                key = mp.MenuKey,
                label = mp.MenuLabel ?? mp.MenuKey,
                path = mp.MenuPath
            })
            .Distinct()
            .ToListAsync();

        return Ok(menus);
    }

    /// <summary>
    /// 获取所有菜单URL列表（用于权限关联）
    /// </summary>
    /// <returns>菜单URL列表</returns>
    [HttpGet("menu-urls")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<ActionResult> GetMenuUrls()
    {
        var menuUrls = await _context.MenuPermissions
            .Where(mp => !string.IsNullOrEmpty(mp.MenuPath))
            .Select(mp => mp.MenuPath!)
            .Distinct()
            .OrderBy(url => url)
            .ToListAsync();

        // 转换为前端需要的格式
        var menuUrlOptions = menuUrls.Select(url => new
        {
            label = url,
            value = url
        }).ToList();

        return Ok(menuUrlOptions);
    }

    /// <summary>
    /// 批量创建或更新角色权限（三级权限树：菜单->子菜单->权限操作）
    /// </summary>
    /// <param name="dto">权限配置DTO</param>
    /// <returns>操作结果</returns>
    [HttpPost("role-permissions")]
    [RequirePermission("menupermission.create")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult> CreateOrUpdateRolePermissions([FromBody] RolePermissionConfigDto dto)
    {
        try
        {
            if (dto == null || string.IsNullOrWhiteSpace(dto.RoleCode))
            {
                return BadRequest(new { message = "角色代码不能为空" });
            }

            // 检查角色是否存在
            var role = await _context.Roles
                .FirstOrDefaultAsync(r => r.Code == dto.RoleCode);
            if (role == null)
            {
                return BadRequest(new { message = "角色不存在" });
            }

            // 获取该角色现有的所有权限
            var existingRolePermissions = await _context.RolePermissions
                .Where(rp => rp.RoleId == role.Id)
                .Include(rp => rp.Permission)
                .ToListAsync();

            // 获取所有选中的权限代码
            var selectedPermissionCodes = dto.PermissionCodes ?? new List<string>();

            // 删除不再需要的 RolePermissions
            var permissionsToRemove = existingRolePermissions
                .Where(rp => !selectedPermissionCodes.Contains(rp.Permission.Code))
                .ToList();
            _context.RolePermissions.RemoveRange(permissionsToRemove);

            // 获取需要添加的权限代码
            var existingPermissionCodes = existingRolePermissions
                .Select(rp => rp.Permission.Code)
                .ToList();
            var permissionCodesToAdd = selectedPermissionCodes
                .Where(code => !existingPermissionCodes.Contains(code))
                .ToList();

            // 为每个权限代码创建或获取 Permission，并创建 RolePermission
            foreach (var permissionCode in permissionCodesToAdd)
            {
                // 查找或创建 Permission
                var permission = await _context.Permissions
                    .FirstOrDefaultAsync(p => p.Code == permissionCode);

                if (permission == null)
                {
                    // 解析权限代码：格式为 {menuKey}.{action}
                    var parts = permissionCode.Split('.');
                    if (parts.Length != 2)
                    {
                        _logger.LogWarning("权限代码格式不正确: {PermissionCode}", permissionCode);
                        continue;
                    }

                    var menuKey = parts[0];
                    var action = parts[1];

                    // 从 MenuPermissions 获取菜单信息（如果存在）
                    var menuPermission = await _context.MenuPermissions
                        .FirstOrDefaultAsync(mp => mp.MenuKey == menuKey);

                    permission = new Permission
                    {
                        Id = Guid.NewGuid(),
                        Name = $"{menuPermission?.MenuLabel ?? menuKey} - {action}",
                        Code = permissionCode,
                        Description = $"{menuPermission?.MenuLabel ?? menuKey} 的 {action} 权限",
                        MenuUrl = menuPermission?.MenuPath,
                        PermissionType = action,
                        IsActive = true,
                        SortOrder = 0,
                        CreatedAt = DateTime.UtcNow,
                        UpdatedAt = DateTime.UtcNow
                    };

                    _context.Permissions.Add(permission);
                }

                // 创建 RolePermission
                var rolePermission = new RolePermission
                {
                    Id = Guid.NewGuid(),
                    RoleId = role.Id,
                    PermissionId = permission.Id,
                    CreatedAt = DateTime.UtcNow
                };

                _context.RolePermissions.Add(rolePermission);
            }

            await _context.SaveChangesAsync();
            _logger.LogInformation("角色权限配置更新成功: RoleCode={RoleCode}, PermissionCount={Count}", 
                dto.RoleCode, selectedPermissionCodes.Count);

            return Ok(new { message = "权限配置更新成功", count = selectedPermissionCodes.Count });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "创建或更新角色权限配置失败");
            return BadRequest(new { message = "操作失败", error = ex.Message });
        }
    }

    /// <summary>
    /// 获取角色的所有权限代码列表
    /// </summary>
    /// <param name="roleCode">角色代码</param>
    /// <returns>权限代码列表</returns>
    [HttpGet("role-permissions/{roleCode}")]
    [RequirePermission("menupermission.view")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<ActionResult> GetRolePermissions(string roleCode)
    {
        var role = await _context.Roles
            .FirstOrDefaultAsync(r => r.Code == roleCode);

        if (role == null)
        {
            return NotFound(new { message = "角色不存在" });
        }

        var permissionCodes = await _context.RolePermissions
            .Where(rp => rp.RoleId == role.Id)
            .Include(rp => rp.Permission)
            .Select(rp => rp.Permission.Code)
            .ToListAsync();

        return Ok(permissionCodes);
    }
}

/// <summary>
/// 创建菜单权限配置DTO
/// </summary>
public class CreateMenuPermissionDto
{
    public string MenuKey { get; set; } = string.Empty;
    public string RoleCode { get; set; } = string.Empty;
    public string? MenuLabel { get; set; }
    public string? MenuPath { get; set; }
    public bool IsActive { get; set; } = true;
    public int SortOrder { get; set; } = 0;
}

/// <summary>
/// 角色权限配置DTO（三级权限树）
/// </summary>
public class RolePermissionConfigDto
{
    /// <summary>
    /// 角色代码
    /// </summary>
    public string RoleCode { get; set; } = string.Empty;

    /// <summary>
    /// 选中的权限代码列表（格式：{menuKey}.{action}，如：dashboard.view, patients.create）
    /// </summary>
    public List<string> PermissionCodes { get; set; } = new List<string>();
}

