using Medical.API.Data;
using Medical.API.Models.DTOs;
using Medical.API.Models.Entities;
using Medical.API.Repositories;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Medical.API.Attributes;

namespace Medical.API.Controllers;

/// <summary>
/// 药品分类控制器
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
[Authorize(Roles = "Admin,SuperAdmin")]
public class DrugCategoriesController : ControllerBase
{
    private readonly MedicalDbContext _context;
    private readonly ILogger<DrugCategoriesController> _logger;


    public DrugCategoriesController(MedicalDbContext context, ILogger<DrugCategoriesController> logger)
    {
        _context = context;
        _logger = logger;
    }

    /// <summary>
    /// 获取药品分类列表（树形结构）
    /// </summary>
    /// <param name="includeInactive">是否包含未启用的分类</param>
    /// <param name="keyword">关键词搜索（分类名称、描述）</param>
    /// <param name="page">页码</param>
    /// <param name="pageSize">每页数量</param>
    /// <returns>分类列表</returns>
    [HttpGet]
    [RequirePermission("drugcategory.view")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<ActionResult> GetDrugCategories(
        [FromQuery] bool includeInactive = false,
        [FromQuery] string? keyword = null,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10)
    {
        var query = _context.DrugCategories.AsQueryable();

        if (!includeInactive)
        {
            query = query.Where(c => c.IsActive);
        }

        // 关键词搜索
        if (!string.IsNullOrEmpty(keyword))
        {
            query = query.Where(c => 
                c.CategoryName.Contains(keyword) ||
                (c.Description != null && c.Description.Contains(keyword)));
        }

        var total = await query.CountAsync();
        var items = await query
            .OrderBy(c => c.SortOrder)
            .ThenBy(c => c.CategoryName)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(c => new
            {
                c.Id,
                c.CategoryName,
                c.ParentId,
                c.Description,
                c.SortOrder,
                c.IsActive,
                c.CreatedAt,
                c.UpdatedAt
            })
            .ToListAsync();

        return Ok(new { items, total, page, pageSize });

        var categories = await query
            .OrderBy(c => c.SortOrder)
            .ThenBy(c => c.CategoryName)
            .Select(c => new
            {
                c.Id,
                c.CategoryName,
                c.ParentId,
                c.Description,
                c.SortOrder,
                c.IsActive,
                c.CreatedAt,
                c.UpdatedAt
            })
            .ToListAsync();

        return Ok(categories);
    }

    /// <summary>
    /// 获取分类树（包含子分类）
    /// </summary>
    /// <returns>分类树</returns>
    [HttpGet("tree")]
    [RequirePermission("drugcategory.view")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<ActionResult> GetCategoryTree()
    {
        var allCategories = await _context.DrugCategories
            .Where(c => c.IsActive)
            .OrderBy(c => c.SortOrder)
            .ThenBy(c => c.CategoryName)
            .Select(c => new CategoryTreeItem
            {
                Id = c.Id,
                CategoryName = c.CategoryName,
                ParentId = c.ParentId,
                Description = c.Description,
                SortOrder = c.SortOrder
            })
            .ToListAsync();

        // 构建树形结构
        var rootCategories = allCategories.Where(c => c.ParentId == null).ToList();
        var tree = rootCategories.Select(c => BuildCategoryTree(c, allCategories)).ToList();

        return Ok(tree);
    }

    private CategoryTreeNode BuildCategoryTree(CategoryTreeItem category, List<CategoryTreeItem> allCategories)
    {
        var children = allCategories
            .Where(c => c.ParentId == category.Id)
            .Select(c => BuildCategoryTree(c, allCategories))
            .ToList();

        return new CategoryTreeNode
        {
            Id = category.Id,
            CategoryName = category.CategoryName,
            ParentId = category.ParentId,
            Description = category.Description,
            SortOrder = category.SortOrder,
            Children = children
        };
    }

    // 辅助类
    private class CategoryTreeItem
    {
        public Guid Id { get; set; }
        public string CategoryName { get; set; } = string.Empty;
        public Guid? ParentId { get; set; }
        public string? Description { get; set; }
        public int SortOrder { get; set; }
    }

    private class CategoryTreeNode
    {
        public Guid Id { get; set; }
        public string CategoryName { get; set; } = string.Empty;
        public Guid? ParentId { get; set; }
        public string? Description { get; set; }
        public int SortOrder { get; set; }
        public List<CategoryTreeNode> Children { get; set; } = new List<CategoryTreeNode>();
    }

    /// <summary>
    /// 根据ID获取分类详情
    /// </summary>
    /// <param name="id">分类ID</param>
    /// <returns>分类详情</returns>
    [HttpGet("{id}")]
    [RequirePermission("drugcategory.view")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<DrugCategory>> GetDrugCategoryById(Guid id)
    {
        var category = await _context.DrugCategories
            .Include(c => c.Parent)
            .FirstOrDefaultAsync(c => c.Id == id);

        if (category == null)
        {
            return NotFound(new { message = "分类不存在" });
        }

        return Ok(category);
    }

    /// <summary>
    /// 创建药品分类
    /// </summary>
    /// <param name="dto">分类信息</param>
    /// <returns>创建的分类</returns>
    [HttpPost]
    [RequirePermission("drugcategory.create")]
    [ProducesResponseType(typeof(DrugCategory), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<DrugCategory>> CreateDrugCategory([FromBody] CreateDrugCategoryDto dto)
    {
        try
        {
            ModelState.Clear();

            if (dto == null)
            {
                return BadRequest(new { message = "请求数据不能为空" });
            }

            if (string.IsNullOrWhiteSpace(dto.CategoryName))
            {
                return BadRequest(new { message = "分类名称不能为空" });
            }

            // 检查分类名称是否已存在
            var existingCategory = await _context.DrugCategories
                .FirstOrDefaultAsync(c => c.CategoryName == dto.CategoryName);
            if (existingCategory != null)
            {
                return BadRequest(new { message = "分类名称已存在" });
            }

            // 如果指定了父分类，验证父分类是否存在
            if (dto.ParentId.HasValue)
            {
                var parentExists = await _context.DrugCategories
                    .AnyAsync(c => c.Id == dto.ParentId.Value);
                if (!parentExists)
                {
                    return BadRequest(new { message = "父分类不存在" });
                }
            }

            var newCategory = new DrugCategory
            {
                Id = Guid.NewGuid(),
                CategoryName = dto.CategoryName,
                ParentId = dto.ParentId,
                Description = string.IsNullOrWhiteSpace(dto.Description) ? null : dto.Description,
                SortOrder = dto.SortOrder,
                IsActive = dto.IsActive,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            _context.DrugCategories.Add(newCategory);
            await _context.SaveChangesAsync();

            _logger.LogInformation("药品分类创建成功: Id={Id}, CategoryName={CategoryName}", 
                newCategory.Id, newCategory.CategoryName);

            return CreatedAtAction(nameof(GetDrugCategoryById), new { id = newCategory.Id }, newCategory);
        }
        catch (DbUpdateException dbEx)
        {
            _logger.LogError(dbEx, "数据库更新失败");
            return BadRequest(new { message = "数据库操作失败", error = dbEx.InnerException?.Message ?? dbEx.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "创建药品分类失败");
            return BadRequest(new { message = "创建药品分类失败", error = ex.Message });
        }
    }

    /// <summary>
    /// 更新药品分类
    /// </summary>
    /// <param name="id">分类ID</param>
    /// <param name="dto">分类信息</param>
    /// <returns>更新后的分类</returns>
    [HttpPut("{id}")]
    [RequirePermission("drugcategory.update")]
    [ProducesResponseType(typeof(DrugCategory), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<DrugCategory>> UpdateDrugCategory(
        Guid id, 
        [FromBody] CreateDrugCategoryDto dto)
    {
        try
        {
            ModelState.Clear();

            var category = await _context.DrugCategories.FindAsync(id);
            if (category == null)
            {
                return NotFound(new { message = "分类不存在" });
            }

            if (dto == null)
            {
                return BadRequest(new { message = "请求数据不能为空" });
            }

            // 如果更改了分类名称，检查是否已存在
            if (dto.CategoryName != category.CategoryName)
            {
                var existingCategory = await _context.DrugCategories
                    .FirstOrDefaultAsync(c => c.CategoryName == dto.CategoryName && c.Id != id);
                if (existingCategory != null)
                {
                    return BadRequest(new { message = "分类名称已存在" });
                }
            }

            // 如果指定了父分类，验证父分类是否存在且不是自己
            if (dto.ParentId.HasValue)
            {
                if (dto.ParentId.Value == id)
                {
                    return BadRequest(new { message = "不能将自己设为父分类" });
                }

                var parentExists = await _context.DrugCategories
                    .AnyAsync(c => c.Id == dto.ParentId.Value);
                if (!parentExists)
                {
                    return BadRequest(new { message = "父分类不存在" });
                }
            }

            category.CategoryName = dto.CategoryName;
            category.ParentId = dto.ParentId;
            category.Description = string.IsNullOrWhiteSpace(dto.Description) ? null : dto.Description;
            category.SortOrder = dto.SortOrder;
            category.IsActive = dto.IsActive;
            category.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            _logger.LogInformation("药品分类更新成功: Id={Id}", id);

            return Ok(category);
        }
        catch (DbUpdateException dbEx)
        {
            _logger.LogError(dbEx, "数据库更新失败");
            return BadRequest(new { message = "数据库操作失败", error = dbEx.InnerException?.Message ?? dbEx.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "更新药品分类失败");
            return BadRequest(new { message = "更新药品分类失败", error = ex.Message });
        }
    }

    /// <summary>
    /// 删除药品分类
    /// </summary>
    /// <param name="id">分类ID</param>
    /// <returns>删除结果</returns>
    [HttpDelete("{id}")]
    [RequirePermission("drugcategory.delete")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteDrugCategory(Guid id)
    {
        var category = await _context.DrugCategories
            .Include(c => c.Children)
            .Include(c => c.Drugs)
            .FirstOrDefaultAsync(c => c.Id == id);

        if (category == null)
        {
            return NotFound(new { message = "分类不存在" });
        }

        // 检查是否有子分类
        if (category.Children.Any())
        {
            return BadRequest(new { message = "该分类下有子分类，无法删除" });
        }

        // 检查是否有药品使用此分类
        if (category.Drugs.Any())
        {
            return BadRequest(new { message = "该分类下有药品，无法删除" });
        }

        _context.DrugCategories.Remove(category);
        await _context.SaveChangesAsync();

        _logger.LogInformation("药品分类删除成功: Id={Id}", id);

        return NoContent();
    }
}

