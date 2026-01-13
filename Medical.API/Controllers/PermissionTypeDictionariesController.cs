using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using Medical.API.Data;
using Medical.API.Models.Entities;
using Medical.API.Attributes;

namespace Medical.API.Controllers;

/// <summary>
/// 权限类型字典控制器
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
[Authorize(Roles = "Admin,SuperAdmin")]
public class PermissionTypeDictionariesController : ControllerBase
{
    private readonly MedicalDbContext _context;
    private readonly ILogger<PermissionTypeDictionariesController> _logger;

    public PermissionTypeDictionariesController(MedicalDbContext context, ILogger<PermissionTypeDictionariesController> logger)
    {
        _context = context;
        _logger = logger;
    }

    /// <summary>
    /// 获取所有权限类型字典（用于下拉列表）
    /// </summary>
    /// <returns>权限类型列表</returns>
    [HttpGet("all")]
    [RequirePermission("permissiontype.view")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<ActionResult> GetAllPermissionTypes()
    {
        var permissionTypes = await _context.PermissionTypeDictionaries
            .Where(pt => pt.IsActive)
            .OrderBy(pt => pt.SortOrder)
            .ThenBy(pt => pt.Name)
            .Select(pt => new
            {
                pt.Id,
                pt.Name,
                pt.Code,
                pt.Description,
                pt.SortOrder
            })
            .ToListAsync();

        return Ok(permissionTypes);
    }

    /// <summary>
    /// 获取权限类型字典列表（分页）
    /// </summary>
    /// <param name="keyword">关键词搜索</param>
    /// <param name="page">页码</param>
    /// <param name="pageSize">每页数量</param>
    /// <returns>权限类型列表</returns>
    [HttpGet]
    [RequirePermission("permissiontype.view")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<ActionResult> GetPermissionTypes(
        [FromQuery] string? keyword = null,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 100)
    {
        var query = _context.PermissionTypeDictionaries.AsQueryable();

        if (!string.IsNullOrEmpty(keyword))
        {
            query = query.Where(pt =>
                pt.Name.Contains(keyword) ||
                pt.Code.Contains(keyword) ||
                (pt.Description != null && pt.Description.Contains(keyword)));
        }

        var total = await query.CountAsync();
        var items = await query
            .OrderBy(pt => pt.SortOrder)
            .ThenBy(pt => pt.Name)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(pt => new
            {
                pt.Id,
                pt.Name,
                pt.Code,
                pt.Description,
                pt.SortOrder,
                pt.IsActive,
                pt.CreatedAt,
                pt.UpdatedAt
            })
            .ToListAsync();

        return Ok(new { items, total, page, pageSize });
    }
}

