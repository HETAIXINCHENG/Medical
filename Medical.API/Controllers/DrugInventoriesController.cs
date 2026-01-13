using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using Medical.API.Data;
using Medical.API.Models.Entities;
using Medical.API.Attributes;

namespace Medical.API.Controllers;

/// <summary>
/// 药品库存控制器
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
[Authorize(Roles = "Admin,SuperAdmin")]
public class DrugInventoriesController : ControllerBase
{
    private readonly MedicalDbContext _context;
    private readonly ILogger<DrugInventoriesController> _logger;

    public DrugInventoriesController(MedicalDbContext context, ILogger<DrugInventoriesController> logger)
    {
        _context = context;
        _logger = logger;
    }

    /// <summary>
    /// 获取药品库存列表
    /// </summary>
    /// <param name="drugId">药品ID（可选）</param>
    /// <param name="warehouseLocation">库位（可选）</param>
    /// <param name="page">页码</param>
    /// <param name="pageSize">每页数量</param>
    /// <returns>库存列表</returns>
    [HttpGet]
    [RequirePermission("druginventory.view")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<ActionResult> GetDrugInventories(
        [FromQuery] Guid? drugId = null,
        [FromQuery] string? warehouseLocation = null,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10)
    {
        var query = _context.DrugInventories
            .Include(i => i.Drug)
                .ThenInclude(d => d.Category)
            .AsQueryable();

        if (drugId.HasValue)
        {
            query = query.Where(i => i.DrugId == drugId.Value);
        }

        if (!string.IsNullOrEmpty(warehouseLocation))
        {
            query = query.Where(i => i.WarehouseLocation == warehouseLocation);
        }

        var total = await query.CountAsync();
        var items = await query
            .OrderByDescending(i => i.LastUpdatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(i => new
            {
                i.Id,
                i.DrugId,
                Drug = new
                {
                    i.Drug.CommonName,
                    i.Drug.TradeName,
                    i.Drug.Specification,
                    i.Drug.Manufacturer,
                    Category = new { i.Drug.Category.CategoryName }
                },
                i.WarehouseLocation,
                i.CurrentQuantity,
                i.LastUpdatedAt
            })
            .ToListAsync();

        return Ok(new { items, total, page, pageSize });
    }

    /// <summary>
    /// 根据ID获取库存详情
    /// </summary>
    /// <param name="id">库存ID</param>
    /// <returns>库存详情</returns>
    [HttpGet("{id}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<DrugInventory>> GetDrugInventoryById(Guid id)
    {
        var inventory = await _context.DrugInventories
            .Include(i => i.Drug)
            .FirstOrDefaultAsync(i => i.Id == id);

        if (inventory == null)
        {
            return NotFound(new { message = "库存记录不存在" });
        }

        return Ok(inventory);
    }

    /// <summary>
    /// 获取药品的总库存（所有库位）
    /// </summary>
    /// <param name="drugId">药品ID</param>
    /// <returns>总库存信息</returns>
    [HttpGet("drug/{drugId}/total")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<ActionResult> GetDrugTotalInventory(Guid drugId)
    {
        var inventories = await _context.DrugInventories
            .Where(i => i.DrugId == drugId)
            .ToListAsync();

        var totalQuantity = inventories.Sum(i => i.CurrentQuantity);
        var warehouseCount = inventories.Count;

        return Ok(new
        {
            DrugId = drugId,
            TotalQuantity = totalQuantity,
            WarehouseCount = warehouseCount,
            Inventories = inventories.Select(i => new
            {
                i.WarehouseLocation,
                i.CurrentQuantity,
                i.LastUpdatedAt
            })
        });
    }
}

