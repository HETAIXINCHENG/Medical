using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using Medical.API.Data;
using Medical.API.Models.Entities;
using Medical.API.Models.DTOs;
using Medical.API.Attributes;

namespace Medical.API.Controllers;

/// <summary>
/// 药品信息控制器
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
[Authorize(Roles = "Admin,SuperAdmin")]
public class DrugsController : ControllerBase
{
    private readonly MedicalDbContext _context;
    private readonly ILogger<DrugsController> _logger;

    public DrugsController(MedicalDbContext context, ILogger<DrugsController> logger)
    {
        _context = context;
        _logger = logger;
    }

    /// <summary>
    /// 获取药品列表
    /// </summary>
    /// <param name="categoryId">分类ID（可选）</param>
    /// <param name="keyword">关键词搜索（通用名、商品名、批准文号）</param>
    /// <param name="page">页码</param>
    /// <param name="pageSize">每页数量</param>
    /// <returns>药品列表</returns>
    [HttpGet]
    [RequirePermission("drug.view")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<ActionResult> GetDrugs(
        [FromQuery] Guid? categoryId = null,
        [FromQuery] string? keyword = null,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10)
    {
        var query = _context.Drugs
            .Include(d => d.Category)
            .AsQueryable();

        // 分类筛选
        if (categoryId.HasValue)
        {
            query = query.Where(d => d.CategoryId == categoryId.Value);
        }

        // 关键词搜索
        if (!string.IsNullOrEmpty(keyword))
        {
            query = query.Where(d => 
                d.CommonName.Contains(keyword) || 
                (d.TradeName != null && d.TradeName.Contains(keyword)) ||
                d.ApprovalNumber.Contains(keyword) ||
                d.Manufacturer.Contains(keyword));
        }

        var total = await query.CountAsync();
        var items = await query
            .OrderBy(d => d.CommonName)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(d => new
            {
                d.Id,
                d.CommonName,
                d.TradeName,
                d.Specification,
                d.Manufacturer,
                d.ApprovalNumber,
                d.CategoryId,
                Category = new { d.Category.CategoryName },
                d.Unit,
                d.StorageCondition,
                d.IsActive,
                d.CreatedAt,
                d.UpdatedAt
            })
            .ToListAsync();

        return Ok(new { items, total, page, pageSize });
    }

    /// <summary>
    /// 根据ID获取药品详情
    /// </summary>
    /// <param name="id">药品ID</param>
    /// <returns>药品详情</returns>
    [HttpGet("{id}")]
    [RequirePermission("drug.view")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<Drug>> GetDrugById(Guid id)
    {
        var drug = await _context.Drugs
            .Include(d => d.Category)
            .FirstOrDefaultAsync(d => d.Id == id);

        if (drug == null)
        {
            return NotFound(new { message = "药品不存在" });
        }

        return Ok(drug);
    }

    /// <summary>
    /// 创建药品
    /// </summary>
    /// <param name="dto">药品信息</param>
    /// <returns>创建的药品</returns>
    [HttpPost]
    [RequirePermission("drug.create")]
    [ProducesResponseType(typeof(Drug), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<Drug>> CreateDrug([FromBody] CreateDrugDto dto)
    {
        try
        {
            ModelState.Clear();

            if (dto == null)
            {
                return BadRequest(new { message = "请求数据不能为空" });
            }

            if (string.IsNullOrWhiteSpace(dto.CommonName))
            {
                return BadRequest(new { message = "药品通用名不能为空" });
            }

            if (string.IsNullOrWhiteSpace(dto.ApprovalNumber))
            {
                return BadRequest(new { message = "国药准字批准文号不能为空" });
            }

            // 检查批准文号是否已存在
            var existingDrug = await _context.Drugs
                .FirstOrDefaultAsync(d => d.ApprovalNumber == dto.ApprovalNumber);
            if (existingDrug != null)
            {
                return BadRequest(new { message = "该批准文号已存在" });
            }

            // 验证分类是否存在
            var categoryExists = await _context.DrugCategories
                .AnyAsync(c => c.Id == dto.CategoryId);
            if (!categoryExists)
            {
                return BadRequest(new { message = "分类不存在" });
            }

            var newDrug = new Drug
            {
                Id = Guid.NewGuid(),
                CommonName = dto.CommonName,
                TradeName = string.IsNullOrWhiteSpace(dto.TradeName) ? null : dto.TradeName,
                Specification = dto.Specification,
                Manufacturer = dto.Manufacturer,
                ApprovalNumber = dto.ApprovalNumber,
                CategoryId = dto.CategoryId,
                Unit = dto.Unit,
                StorageCondition = dto.StorageCondition ?? "常温",
                IsActive = dto.IsActive,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            _context.Drugs.Add(newDrug);
            await _context.SaveChangesAsync();

            // 重新加载包含分类信息的药品
            var createdDrug = await _context.Drugs
                .Include(d => d.Category)
                .FirstOrDefaultAsync(d => d.Id == newDrug.Id);

            _logger.LogInformation("药品创建成功: Id={Id}, CommonName={CommonName}", 
                newDrug.Id, newDrug.CommonName);

            return CreatedAtAction(nameof(GetDrugById), new { id = newDrug.Id }, createdDrug);
        }
        catch (DbUpdateException dbEx)
        {
            _logger.LogError(dbEx, "数据库更新失败");
            return BadRequest(new { message = "数据库操作失败", error = dbEx.InnerException?.Message ?? dbEx.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "创建药品失败");
            return BadRequest(new { message = "创建药品失败", error = ex.Message });
        }
    }

    /// <summary>
    /// 更新药品
    /// </summary>
    /// <param name="id">药品ID</param>
    /// <param name="dto">药品信息</param>
    /// <returns>更新后的药品</returns>
    [HttpPut("{id}")]
    [RequirePermission("drug.update")]
    [ProducesResponseType(typeof(Drug), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<Drug>> UpdateDrug(
        Guid id, 
        [FromBody] CreateDrugDto dto)
    {
        try
        {
            ModelState.Clear();

            var drug = await _context.Drugs.FindAsync(id);
            if (drug == null)
            {
                return NotFound(new { message = "药品不存在" });
            }

            if (dto == null)
            {
                return BadRequest(new { message = "请求数据不能为空" });
            }

            // 如果更改了批准文号，检查是否已存在
            if (dto.ApprovalNumber != drug.ApprovalNumber)
            {
                var existingDrug = await _context.Drugs
                    .FirstOrDefaultAsync(d => d.ApprovalNumber == dto.ApprovalNumber && d.Id != id);
                if (existingDrug != null)
                {
                    return BadRequest(new { message = "该批准文号已存在" });
                }
            }

            // 验证分类是否存在
            var categoryExists = await _context.DrugCategories
                .AnyAsync(c => c.Id == dto.CategoryId);
            if (!categoryExists)
            {
                return BadRequest(new { message = "分类不存在" });
            }

            drug.CommonName = dto.CommonName;
            drug.TradeName = string.IsNullOrWhiteSpace(dto.TradeName) ? null : dto.TradeName;
            drug.Specification = dto.Specification;
            drug.Manufacturer = dto.Manufacturer;
            drug.ApprovalNumber = dto.ApprovalNumber;
            drug.CategoryId = dto.CategoryId;
            drug.Unit = dto.Unit;
            drug.StorageCondition = dto.StorageCondition ?? "常温";
            drug.IsActive = dto.IsActive;
            drug.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            // 重新加载包含分类信息的药品
            var updatedDrug = await _context.Drugs
                .Include(d => d.Category)
                .FirstOrDefaultAsync(d => d.Id == id);

            _logger.LogInformation("药品更新成功: Id={Id}", id);

            return Ok(updatedDrug);
        }
        catch (DbUpdateException dbEx)
        {
            _logger.LogError(dbEx, "数据库更新失败");
            return BadRequest(new { message = "数据库操作失败", error = dbEx.InnerException?.Message ?? dbEx.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "更新药品失败");
            return BadRequest(new { message = "更新药品失败", error = ex.Message });
        }
    }

    /// <summary>
    /// 删除药品（逻辑删除）
    /// </summary>
    /// <param name="id">药品ID</param>
    /// <returns>删除结果</returns>
    [HttpDelete("{id}")]
    [RequirePermission("drug.delete")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteDrug(Guid id)
    {
        var drug = await _context.Drugs.FindAsync(id);
        if (drug == null)
        {
            return NotFound(new { message = "药品不存在" });
        }

        // 逻辑删除
        drug.IsActive = false;
        drug.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        _logger.LogInformation("药品删除成功: Id={Id}", id);

        return NoContent();
    }
}

