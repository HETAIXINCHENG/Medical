using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Medical.API.Attributes;
using Medical.API.Data;
using Medical.API.Models.Entities;
using Medical.API.Models.DTOs;

namespace Medical.API.Controllers;

/// <summary>
/// 疾病分类控制器
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class DiseaseCategoriesController : ControllerBase
{
    private readonly MedicalDbContext _context;
    private readonly ILogger<DiseaseCategoriesController> _logger;

    public DiseaseCategoriesController(MedicalDbContext context, ILogger<DiseaseCategoriesController> logger)
    {
        _context = context;
        _logger = logger;
    }

    /// <summary>
    /// 获取所有科室及其疾病分类（树形结构，支持分页）
    /// </summary>
    [HttpGet("tree")]
    [RequirePermission("disease-categories.view")]
    public async Task<ActionResult> GetTree(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 5)
    {
        try
        {
            var query = _context.Departments
                .OrderBy(d => d.SortOrder)
                .ThenBy(d => d.Name)
                .AsQueryable();

            var total = await query.CountAsync();

            var departments = await query
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Include(d => d.DiseaseCategories.OrderBy(dc => dc.Name))
                .Select(d => new
                {
                    id = d.Id,
                    name = d.Name,
                    sortOrder = d.SortOrder,
                    children = d.DiseaseCategories.Select(dc => new
                    {
                        id = dc.Id,
                        name = dc.Name,
                        symptoms = dc.Symptoms,
                        departmentId = dc.DepartmentId,
                        createdAt = dc.CreatedAt
                    }).ToList()
                })
                .ToListAsync();

            return Ok(new { items = departments, total, page, pageSize });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "获取数据失败", error = ex.Message });
        }
    }

    [HttpGet]
    [AllowAnonymous] // 允许匿名访问，患者端需要查看疾病分类
    public async Task<ActionResult> GetList(
        [FromQuery] Guid? departmentId = null,
        [FromQuery] string? keyword = null,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20)
    {
        var query = _context.DiseaseCategories
            .Include(d => d.Department)
            .AsQueryable();

        if (departmentId.HasValue)
        {
            query = query.Where(d => d.DepartmentId == departmentId.Value);
        }

        if (!string.IsNullOrEmpty(keyword))
        {
            query = query.Where(d => 
                d.Name.Contains(keyword) ||
                (d.Symptoms != null && d.Symptoms.Contains(keyword)));
        }

        var total = await query.CountAsync();
        var items = await query
            .OrderBy(d => d.Department.Name)
            .ThenBy(d => d.Name)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(d => new
            {
                d.Id,
                d.DepartmentId,
                DepartmentName = d.Department.Name,
                d.Name,
                d.Symptoms,
                d.CreatedAt
            })
            .ToListAsync();

        return Ok(new { items, total, page, pageSize });
    }

    [HttpGet("{id}")]
    [RequirePermission("disease-categories.view")]
    public async Task<ActionResult<DiseaseCategory>> Get(Guid id)
    {
        var entity = await _context.DiseaseCategories
            .Include(d => d.Department)
            .FirstOrDefaultAsync(d => d.Id == id);
        if (entity == null) return NotFound();
        return Ok(entity);
    }

    [HttpPost]
    [RequirePermission("disease-categories.create")]
    public async Task<ActionResult<DiseaseCategory>> Create([FromBody] CreateDiseaseCategoryDto dto)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        // 验证科室是否存在
        var department = await _context.Departments.FindAsync(dto.DepartmentId);
        if (department == null)
        {
            return BadRequest(new { message = "科室不存在" });
        }

        var entity = new DiseaseCategory
        {
            Id = Guid.NewGuid(),
            DepartmentId = dto.DepartmentId,
            Name = dto.Name.Trim(),
            Symptoms = string.IsNullOrWhiteSpace(dto.Symptoms) ? null : dto.Symptoms.Trim(),
            CreatedAt = DateTime.UtcNow
        };

        _context.DiseaseCategories.Add(entity);
        await _context.SaveChangesAsync();
        return Ok(entity);
    }

    [HttpPut("{id}")]
    [RequirePermission("disease-categories.update")]
    public async Task<ActionResult> Update(Guid id, [FromBody] CreateDiseaseCategoryDto dto)
    {
        _logger.LogInformation("更新疾病分类，ID: {Id}, 接收到的数据: DepartmentId={DepartmentId}, Name={Name}, Symptoms={Symptoms}", 
            id, dto?.DepartmentId, dto?.Name, dto?.Symptoms);

        if (!ModelState.IsValid)
        {
            var errors = ModelState
                .Where(x => x.Value?.Errors.Count > 0)
                .Select(x => new { field = x.Key, errors = x.Value?.Errors.Select(e => e.ErrorMessage) })
                .ToList();
            _logger.LogWarning("更新疾病分类失败：数据验证失败，错误: {Errors}", System.Text.Json.JsonSerializer.Serialize(errors));
            return BadRequest(new { message = "数据验证失败", errors });
        }

        var entity = await _context.DiseaseCategories.FindAsync(id);
        if (entity == null) return NotFound();

        // 验证科室是否存在
        if (dto.DepartmentId != entity.DepartmentId)
        {
            var department = await _context.Departments.FindAsync(dto.DepartmentId);
            if (department == null)
            {
                return BadRequest(new { message = $"科室不存在，ID: {dto.DepartmentId}" });
            }
        }

        entity.DepartmentId = dto.DepartmentId;
        entity.Name = dto.Name.Trim();
        entity.Symptoms = string.IsNullOrWhiteSpace(dto.Symptoms) ? null : dto.Symptoms.Trim();

        _context.DiseaseCategories.Update(entity);
        await _context.SaveChangesAsync();
        return Ok(entity);
    }

    [HttpDelete("{id}")]
    [RequirePermission("disease-categories.delete")]
    public async Task<ActionResult> Delete(Guid id)
    {
        var entity = await _context.DiseaseCategories.FindAsync(id);
        if (entity == null) return NotFound();
        _context.DiseaseCategories.Remove(entity);
        await _context.SaveChangesAsync();
        return NoContent();
    }
}


