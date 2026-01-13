using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using Medical.API.Data;
using Medical.API.Models.Entities;
using Medical.API.Attributes;

namespace Medical.API.Controllers;

/// <summary>
/// 科室控制器
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class DepartmentsController : ControllerBase
{
    private readonly MedicalDbContext _context;
    private readonly ILogger<DepartmentsController> _logger;

    public DepartmentsController(MedicalDbContext context, ILogger<DepartmentsController> logger)
    {
        _context = context;
        _logger = logger;
    }

    /// <summary>
    /// 获取所有科室
    /// </summary>
    /// <param name="isHot">是否只获取热门科室</param>
    /// <param name="keyword">关键词搜索（科室名称、描述）</param>
    /// <param name="page">页码</param>
    /// <param name="pageSize">每页数量</param>
    /// <returns>科室列表</returns>
    [HttpGet]
    [AllowAnonymous] // 允许匿名访问，患者端需要显示科室列表
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<ActionResult> GetDepartments(
        [FromQuery] bool? isHot = null,
        [FromQuery] string? keyword = null,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10)
    {
        var query = _context.Departments.AsQueryable();

        if (isHot.HasValue && isHot.Value)
        {
            query = query.Where(d => d.IsHot);
        }

        // 关键词搜索
        if (!string.IsNullOrEmpty(keyword))
        {
            query = query.Where(d => 
                d.Name.Contains(keyword) ||
                (d.Description != null && d.Description.Contains(keyword)));
        }

        var total = await query.CountAsync();
        var items = await query
            .OrderBy(d => d.SortOrder)
            .ThenBy(d => d.Name)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return Ok(new { items, total, page, pageSize });
    }

    /// <summary>
    /// 根据ID获取科室详情
    /// </summary>
    /// <param name="id">科室ID</param>
    /// <returns>科室详情（包含疾病分类）</returns>
    [HttpGet("{id}")]
    [RequirePermission("department.view")]
    [ProducesResponseType(typeof(Department), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<Department>> GetDepartment(Guid id)
    {
        var department = await _context.Departments
            .Include(d => d.DiseaseCategories)
            .FirstOrDefaultAsync(d => d.Id == id);

        if (department == null)
        {
            return NotFound(new { message = "科室不存在" });
        }

        return Ok(department);
    }

    /// <summary>
    /// 获取科室的疾病分类
    /// </summary>
    /// <param name="id">科室ID</param>
    /// <returns>疾病分类列表</returns>
    [HttpGet("{id}/diseases")]
    [AllowAnonymous] // 允许匿名访问，患者端需要查看疾病分类
    [ProducesResponseType(typeof(List<DiseaseCategory>), StatusCodes.Status200OK)]
    public async Task<ActionResult<List<DiseaseCategory>>> GetDepartmentDiseases(Guid id)
    {
        var diseases = await _context.DiseaseCategories
            .Where(d => d.DepartmentId == id)
            .OrderBy(d => d.Name)
            .ToListAsync();

        return Ok(diseases);
    }

    /// <summary>
    /// 创建科室
    /// </summary>
    /// <param name="department">科室信息</param>
    /// <returns>创建的科室</returns>
    [HttpPost]
    [RequirePermission("department.create")]
    [Authorize(Roles = "Admin,SuperAdmin")]
    [ProducesResponseType(typeof(Department), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<Department>> CreateDepartment([FromBody] Department department)
    {
        if (string.IsNullOrWhiteSpace(department.Name))
        {
            return BadRequest(new { message = "科室名称不能为空" });
        }

        department.Id = Guid.NewGuid();
        department.CreatedAt = DateTime.UtcNow;
        department.UpdatedAt = DateTime.UtcNow;

        _context.Departments.Add(department);
        await _context.SaveChangesAsync();

        return CreatedAtAction(nameof(GetDepartment), new { id = department.Id }, department);
    }

    /// <summary>
    /// 更新科室
    /// </summary>
    /// <param name="id">科室ID</param>
    /// <param name="department">科室信息</param>
    /// <returns>更新后的科室</returns>
    [HttpPut("{id}")]
    [RequirePermission("department.update")]
    [Authorize(Roles = "Admin,SuperAdmin")]
    [ProducesResponseType(typeof(Department), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<Department>> UpdateDepartment(Guid id, [FromBody] Department department)
    {
        var existingDepartment = await _context.Departments.FindAsync(id);
        if (existingDepartment == null)
        {
            return NotFound(new { message = "科室不存在" });
        }

        existingDepartment.Name = department.Name;
        existingDepartment.Description = department.Description;
        existingDepartment.SortOrder = department.SortOrder;
        existingDepartment.IsHot = department.IsHot;
        existingDepartment.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        return Ok(existingDepartment);
    }

    /// <summary>
    /// 删除科室
    /// </summary>
    /// <param name="id">科室ID</param>
    /// <returns>删除结果</returns>
    [HttpDelete("{id}")]
    [RequirePermission("department.delete")]
    [Authorize(Roles = "Admin,SuperAdmin")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteDepartment(Guid id)
    {
        var department = await _context.Departments.FindAsync(id);
        if (department == null)
        {
            return NotFound(new { message = "科室不存在" });
        }

        _context.Departments.Remove(department);
        await _context.SaveChangesAsync();

        return NoContent();
    }
}

