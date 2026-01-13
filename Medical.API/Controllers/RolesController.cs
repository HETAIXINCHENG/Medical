using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using Medical.API.Data;
using Medical.API.Models.Entities;
using Medical.API.Models.DTOs;
using Medical.API.Attributes;

namespace Medical.API.Controllers;

/// <summary>
/// 角色管理控制器
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
[Authorize(Roles = "Admin,SuperAdmin")]
public class RolesController : ControllerBase
{
    private readonly MedicalDbContext _context;
    private readonly ILogger<RolesController> _logger;

    public RolesController(MedicalDbContext context, ILogger<RolesController> logger)
    {
        _context = context;
        _logger = logger;
    }

    /// <summary>
    /// 获取角色列表
    /// </summary>
    /// <param name="keyword">关键词搜索（角色名称、代码、描述）</param>
    /// <param name="page">页码</param>
    /// <param name="pageSize">每页数量</param>
    /// <returns>角色列表</returns>
    [HttpGet]
    [RequirePermission("role.view")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<ActionResult> GetRoles(
        [FromQuery] string? keyword = null,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10)
    {
        var query = _context.Roles.AsQueryable();

        // 关键词搜索
        if (!string.IsNullOrEmpty(keyword))
        {
            query = query.Where(r => 
                r.Name.Contains(keyword) ||
                r.Code.Contains(keyword) ||
                (r.Description != null && r.Description.Contains(keyword)));
        }

        var total = await query.CountAsync();
        var items = await query
            .OrderBy(r => r.SortOrder)
            .ThenBy(r => r.Name)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(r => new
            {
                r.Id,
                r.Name,
                r.Code,
                r.Description,
                r.IsActive,
                r.SortOrder,
                r.CreatedAt,
                r.UpdatedAt
            })
            .ToListAsync();

        return Ok(new { items, total, page, pageSize });
    }

    /// <summary>
    /// 获取所有角色（不分页，用于下拉选择）
    /// </summary>
    /// <returns>角色列表</returns>
    [HttpGet("all")]
    [RequirePermission("role.view")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<ActionResult> GetAllRoles()
    {
        var roles = await _context.Roles
            .Where(r => r.IsActive)
            .OrderBy(r => r.SortOrder)
            .ThenBy(r => r.Name)
            .Select(r => new
            {
                r.Id,
                r.Name,
                r.Code,
                r.Description
            })
            .ToListAsync();

        return Ok(roles);
    }

    /// <summary>
    /// 根据ID获取角色详情（包含权限）
    /// </summary>
    /// <param name="id">角色ID</param>
    /// <returns>角色详情</returns>
    [HttpGet("{id}")]
    [RequirePermission("role.view")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<Role>> GetRoleById(Guid id)
    {
        var role = await _context.Roles
            .Include(r => r.RolePermissions)
                .ThenInclude(rp => rp.Permission)
            .FirstOrDefaultAsync(r => r.Id == id);

        if (role == null)
        {
            return NotFound(new { message = "角色不存在" });
        }

        return Ok(role);
    }

    /// <summary>
    /// 创建角色
    /// </summary>
    /// <param name="dto">角色信息</param>
    /// <returns>创建的角色</returns>
    [HttpPost]
    [RequirePermission("role.create")]
    [ProducesResponseType(typeof(Role), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<Role>> CreateRole([FromBody] CreateRoleDto dto)
    {
        try
        {
            ModelState.Clear();

            if (dto == null)
            {
                return BadRequest(new { message = "请求数据不能为空" });
            }

            if (string.IsNullOrWhiteSpace(dto.Name))
            {
                return BadRequest(new { message = "角色名称不能为空" });
            }

            if (string.IsNullOrWhiteSpace(dto.Code))
            {
                return BadRequest(new { message = "角色代码不能为空" });
            }

            // 检查角色代码是否已存在
            var existingRole = await _context.Roles
                .FirstOrDefaultAsync(r => r.Code == dto.Code);
            if (existingRole != null)
            {
                return BadRequest(new { message = "角色代码已存在" });
            }

            var newRole = new Role
            {
                Id = Guid.NewGuid(),
                Name = dto.Name,
                Code = dto.Code,
                Description = string.IsNullOrWhiteSpace(dto.Description) ? null : dto.Description,
                IsActive = dto.IsActive,
                SortOrder = dto.SortOrder,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            _context.Roles.Add(newRole);
            await _context.SaveChangesAsync();

            _logger.LogInformation("角色创建成功: Id={Id}, Code={Code}", newRole.Id, newRole.Code);

            return CreatedAtAction(nameof(GetRoleById), new { id = newRole.Id }, newRole);
        }
        catch (DbUpdateException dbEx)
        {
            _logger.LogError(dbEx, "数据库更新失败");
            return BadRequest(new { message = "数据库操作失败", error = dbEx.InnerException?.Message ?? dbEx.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "创建角色失败");
            return BadRequest(new { message = "创建角色失败", error = ex.Message });
        }
    }

    /// <summary>
    /// 更新角色
    /// </summary>
    /// <param name="id">角色ID</param>
    /// <param name="dto">角色信息</param>
    /// <returns>更新后的角色</returns>
    [HttpPut("{id}")]
    [RequirePermission("role.update")]
    [ProducesResponseType(typeof(Role), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<Role>> UpdateRole(
        Guid id, 
        [FromBody] CreateRoleDto dto)
    {
        try
        {
            ModelState.Clear();

            var role = await _context.Roles.FindAsync(id);
            if (role == null)
            {
                return NotFound(new { message = "角色不存在" });
            }

            if (dto == null)
            {
                return BadRequest(new { message = "请求数据不能为空" });
            }

            // 如果更改了角色代码，检查是否已存在
            if (dto.Code != role.Code)
            {
                var existingRole = await _context.Roles
                    .FirstOrDefaultAsync(r => r.Code == dto.Code && r.Id != id);
                if (existingRole != null)
                {
                    return BadRequest(new { message = "角色代码已存在" });
                }
            }

            role.Name = dto.Name;
            role.Code = dto.Code;
            role.Description = string.IsNullOrWhiteSpace(dto.Description) ? null : dto.Description;
            role.IsActive = dto.IsActive;
            role.SortOrder = dto.SortOrder;
            role.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            _logger.LogInformation("角色更新成功: Id={Id}", id);

            return Ok(role);
        }
        catch (DbUpdateException dbEx)
        {
            _logger.LogError(dbEx, "数据库更新失败");
            return BadRequest(new { message = "数据库操作失败", error = dbEx.InnerException?.Message ?? dbEx.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "更新角色失败");
            return BadRequest(new { message = "更新角色失败", error = ex.Message });
        }
    }

    /// <summary>
    /// 删除角色
    /// </summary>
    /// <param name="id">角色ID</param>
    /// <returns>删除结果</returns>
    [HttpDelete("{id}")]
    [RequirePermission("role.delete")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteRole(Guid id)
    {
        var role = await _context.Roles.FindAsync(id);
        if (role == null)
        {
            return NotFound(new { message = "角色不存在" });
        }

        // 检查是否有用户使用此角色
        var hasUsers = await _context.UserRoles.AnyAsync(ur => ur.RoleId == id);
        if (hasUsers)
        {
            return BadRequest(new { message = "该角色正在被使用，无法删除" });
        }

        _context.Roles.Remove(role);
        await _context.SaveChangesAsync();

        _logger.LogInformation("角色删除成功: Id={Id}", id);

        return NoContent();
    }

    /// <summary>
    /// 为角色分配权限
    /// </summary>
    /// <param name="id">角色ID</param>
    /// <param name="dto">权限ID列表</param>
    /// <returns>分配结果</returns>
    [HttpPost("{id}/permissions")]
    [RequirePermission("role.update")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult> AssignPermissions(
        Guid id,
        [FromBody] AssignPermissionsDto dto)
    {
        var role = await _context.Roles.FindAsync(id);
        if (role == null)
        {
            return NotFound(new { message = "角色不存在" });
        }

        // 删除现有权限
        var existingPermissions = await _context.RolePermissions
            .Where(rp => rp.RoleId == id)
            .ToListAsync();
        _context.RolePermissions.RemoveRange(existingPermissions);

        // 添加新权限
        if (dto.PermissionIds != null && dto.PermissionIds.Count > 0)
        {
            var permissions = await _context.Permissions
                .Where(p => dto.PermissionIds.Contains(p.Id))
                .ToListAsync();

            foreach (var permission in permissions)
            {
                var rolePermission = new RolePermission
                {
                    Id = Guid.NewGuid(),
                    RoleId = id,
                    PermissionId = permission.Id,
                    CreatedAt = DateTime.UtcNow
                };
                _context.RolePermissions.Add(rolePermission);
            }
        }

        await _context.SaveChangesAsync();

        _logger.LogInformation("角色权限分配成功: RoleId={RoleId}, PermissionCount={Count}", 
            id, dto.PermissionIds?.Count ?? 0);

        return Ok(new { message = "权限分配成功" });
    }

    /// <summary>
    /// 获取角色的权限列表
    /// </summary>
    /// <param name="id">角色ID</param>
    /// <returns>权限列表</returns>
    [HttpGet("{id}/permissions")]
    [RequirePermission("role.view")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult> GetRolePermissions(Guid id)
    {
        var role = await _context.Roles.FindAsync(id);
        if (role == null)
        {
            return NotFound(new { message = "角色不存在" });
        }

        var permissions = await _context.RolePermissions
            .Where(rp => rp.RoleId == id)
            .Include(rp => rp.Permission)
            .Select(rp => new
            {
                rp.Permission.Id,
                rp.Permission.Name,
                rp.Permission.Code,
                rp.Permission.Description,
            })
            .ToListAsync();

        return Ok(permissions);
    }
}

