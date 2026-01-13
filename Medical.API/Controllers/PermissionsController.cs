using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using Medical.API.Data;
using Medical.API.Models.Entities;
using Medical.API.Models.DTOs;
using Medical.API.Attributes;
using System.ComponentModel.DataAnnotations;

namespace Medical.API.Controllers;

/// <summary>
/// 权限管理控制器
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
[Authorize(Roles = "Admin,SuperAdmin")]
public class PermissionsController : ControllerBase
{
    private readonly MedicalDbContext _context;
    private readonly ILogger<PermissionsController> _logger;

    public PermissionsController(MedicalDbContext context, ILogger<PermissionsController> logger)
    {
        _context = context;
        _logger = logger;
    }

    /// <summary>
    /// 获取权限列表
    /// </summary>
    /// <param name="menuUrl">菜单URL（可选）</param>
    /// <param name="keyword">关键词搜索（名称、代码、描述）</param>
    /// <param name="page">页码</param>
    /// <param name="pageSize">每页数量</param>
    /// <returns>权限列表</returns>
    [HttpGet]
    [RequirePermission("permission.view")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<ActionResult> GetPermissions(
        [FromQuery] string? menuUrl = null,
        [FromQuery] string? keyword = null,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 100)
    {
        var query = _context.Permissions.AsQueryable();

        if (!string.IsNullOrEmpty(menuUrl))
        {
            query = query.Where(p => p.MenuUrl == menuUrl);
        }

        // 关键词搜索
        if (!string.IsNullOrEmpty(keyword))
        {
            query = query.Where(p =>
                p.Name.Contains(keyword) ||
                p.Code.Contains(keyword) ||
                (p.Description != null && p.Description.Contains(keyword)));
        }

        var total = await query.CountAsync();
        var items = await query
            .OrderBy(p => p.SortOrder)
            .ThenBy(p => p.Name)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(p => new
            {
                p.Id,
                p.Name,
                p.Code,
                p.Description,
                p.MenuUrl,
                p.PermissionType,
                p.SortOrder,
                p.IsActive,
                p.CreatedAt,
                p.UpdatedAt
            })
            .ToListAsync();

        return Ok(new { items, total, page, pageSize });
    }

    /// <summary>
    /// 获取所有权限（不分页，用于权限分配）
    /// </summary>
    /// <param name="menuUrl">菜单URL（可选）</param>
    /// <returns>权限列表</returns>
    [HttpGet("all")]
    [RequirePermission("permission.view")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<ActionResult> GetAllPermissions(
        [FromQuery] string? menuUrl = null)
    {
        var query = _context.Permissions
            .Where(p => p.IsActive)
            .AsQueryable();

        if (!string.IsNullOrEmpty(menuUrl))
        {
            query = query.Where(p => p.MenuUrl == menuUrl);
        }

        var permissions = await query
            .OrderBy(p => p.SortOrder)
            .ThenBy(p => p.Name)
            .Select(p => new
            {
                p.Id,
                p.Name,
                p.Code,
                p.Description,
                p.MenuUrl,
                p.PermissionType
            })
            .ToListAsync();

        return Ok(permissions);
    }

    /// <summary>
    /// 获取所有菜单URL列表（用于权限关联）
    /// </summary>
    /// <returns>菜单URL列表</returns>
    [HttpGet("menu-urls")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<ActionResult> GetMenuUrls()
    {
        // 从 MenuPermissions 表获取所有菜单路径
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
    /// 根据ID获取权限详情
    /// </summary>
    /// <param name="id">权限ID</param>
    /// <returns>权限详情</returns>
    [HttpGet("{id}")]
    [RequirePermission("permission.view")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult> GetPermissionById(Guid id)
    {
        var permission = await _context.Permissions.FindAsync(id);
        if (permission == null)
        {
            return NotFound(new { message = "权限不存在" });
        }

        return Ok(new
        {
            permission.Id,
            permission.Name,
            permission.Code,
            permission.Description,
            permission.MenuUrl,
            permission.PermissionType,
            permission.SortOrder,
            permission.IsActive,
            permission.CreatedAt,
            permission.UpdatedAt
        });
    }

    /// <summary>
    /// 创建权限
    /// </summary>
    /// <param name="dto">权限信息</param>
    /// <returns>创建的权限</returns>
    [HttpPost]
    [RequirePermission("permission.create")]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult> CreatePermission([FromBody] CreatePermissionDto dto)
    {
        try
        {
            if (dto == null)
            {
                return BadRequest(new { message = "请求数据不能为空" });
            }

            if (string.IsNullOrWhiteSpace(dto.Name))
            {
                return BadRequest(new { message = "权限名称不能为空" });
            }

            if (string.IsNullOrWhiteSpace(dto.Code))
            {
                return BadRequest(new { message = "权限代码不能为空" });
            }

            // 检查权限代码是否已存在
            var existingPermission = await _context.Permissions
                .FirstOrDefaultAsync(p => p.Code == dto.Code);
            if (existingPermission != null)
            {
                return BadRequest(new { message = "权限代码已存在" });
            }

            var newPermission = new Permission
            {
                Id = Guid.NewGuid(),
                Name = dto.Name,
                Code = dto.Code,
                Description = string.IsNullOrWhiteSpace(dto.Description) ? null : dto.Description,
                MenuUrl = string.IsNullOrWhiteSpace(dto.MenuUrl) ? null : dto.MenuUrl,
                PermissionType = string.IsNullOrWhiteSpace(dto.PermissionType) ? null : dto.PermissionType,
                SortOrder = dto.SortOrder ?? 0,
                IsActive = dto.IsActive ?? true,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            _context.Permissions.Add(newPermission);
            await _context.SaveChangesAsync();

            _logger.LogInformation("权限创建成功: Id={Id}, Code={Code}", newPermission.Id, newPermission.Code);

            return CreatedAtAction(nameof(GetPermissionById), new { id = newPermission.Id }, new
            {
                newPermission.Id,
                newPermission.Name,
                newPermission.Code,
                newPermission.Description,
                newPermission.MenuUrl,
                newPermission.PermissionType,
                newPermission.SortOrder,
                newPermission.IsActive,
                newPermission.CreatedAt,
                newPermission.UpdatedAt
            });
        }
        catch (DbUpdateException dbEx)
        {
            _logger.LogError(dbEx, "数据库更新失败");
            return BadRequest(new { message = "数据库操作失败", error = dbEx.InnerException?.Message ?? dbEx.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "创建权限失败");
            return BadRequest(new { message = "创建权限失败", error = ex.Message });
        }
    }

    /// <summary>
    /// 更新权限
    /// </summary>
    /// <param name="id">权限ID</param>
    /// <param name="dto">权限信息</param>
    /// <returns>更新后的权限</returns>
    [HttpPut("{id}")]
    [RequirePermission("permission.update")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult> UpdatePermission(
        Guid id,
        [FromBody] CreatePermissionDto dto)
    {
        try
        {
            var permission = await _context.Permissions.FindAsync(id);
            if (permission == null)
            {
                return NotFound(new { message = "权限不存在" });
            }

            if (dto == null)
            {
                return BadRequest(new { message = "请求数据不能为空" });
            }

            // 如果更改了权限代码，检查是否已存在
            if (dto.Code != permission.Code)
            {
                var existingPermission = await _context.Permissions
                    .FirstOrDefaultAsync(p => p.Code == dto.Code && p.Id != id);
                if (existingPermission != null)
                {
                    return BadRequest(new { message = "权限代码已存在" });
                }
            }

            permission.Name = dto.Name;
            permission.Code = dto.Code;
            permission.Description = string.IsNullOrWhiteSpace(dto.Description) ? null : dto.Description;
            permission.MenuUrl = string.IsNullOrWhiteSpace(dto.MenuUrl) ? null : dto.MenuUrl;
            permission.PermissionType = string.IsNullOrWhiteSpace(dto.PermissionType) ? null : dto.PermissionType;
            permission.SortOrder = dto.SortOrder ?? permission.SortOrder;
            permission.IsActive = dto.IsActive ?? permission.IsActive;
            permission.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            _logger.LogInformation("权限更新成功: Id={Id}", id);

            return Ok(new
            {
                permission.Id,
                permission.Name,
                permission.Code,
                permission.Description,
                permission.MenuUrl,
                permission.PermissionType,
                permission.SortOrder,
                permission.IsActive,
                permission.CreatedAt,
                permission.UpdatedAt
            });
        }
        catch (DbUpdateException dbEx)
        {
            _logger.LogError(dbEx, "数据库更新失败");
            return BadRequest(new { message = "数据库操作失败", error = dbEx.InnerException?.Message ?? dbEx.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "更新权限失败");
            return BadRequest(new { message = "更新权限失败", error = ex.Message });
        }
    }

    /// <summary>
    /// 删除权限
    /// </summary>
    /// <param name="id">权限ID</param>
    /// <returns>删除结果</returns>
    [HttpDelete("{id}")]
    [RequirePermission("permission.delete")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeletePermission(Guid id)
    {
        var permission = await _context.Permissions.FindAsync(id);
        if (permission == null)
        {
            return NotFound(new { message = "权限不存在" });
        }

        // 先删除所有相关的角色权限关联（级联删除）
        var rolePermissions = await _context.RolePermissions
            .Where(rp => rp.PermissionId == id)
            .ToListAsync();
        
        if (rolePermissions.Count > 0)
        {
            _context.RolePermissions.RemoveRange(rolePermissions);
            _logger.LogInformation("删除权限时，同时删除了 {Count} 个角色权限关联", rolePermissions.Count);
        }

        // 删除权限本身
        _context.Permissions.Remove(permission);
        await _context.SaveChangesAsync();

        _logger.LogInformation("权限删除成功: Id={Id}, Code={Code}", id, permission.Code);

        return NoContent();
    }
}

/// <summary>
/// 创建/更新权限DTO
/// </summary>
public class CreatePermissionDto
{
    [Required(ErrorMessage = "权限名称不能为空")]
    [MaxLength(100, ErrorMessage = "权限名称长度不能超过100个字符")]
    public string Name { get; set; } = string.Empty;

    [Required(ErrorMessage = "权限代码不能为空")]
    [MaxLength(100, ErrorMessage = "权限代码长度不能超过100个字符")]
    public string Code { get; set; } = string.Empty;

    [MaxLength(500, ErrorMessage = "权限描述长度不能超过500个字符")]
    public string? Description { get; set; }

    [MaxLength(200, ErrorMessage = "菜单URL长度不能超过200个字符")]
    public string? MenuUrl { get; set; }

    [MaxLength(50, ErrorMessage = "权限类型长度不能超过50个字符")]
    public string? PermissionType { get; set; }

    public int? SortOrder { get; set; }

    public bool? IsActive { get; set; }
}

