using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using Medical.API.Data;
using Medical.API.Models.Entities;
using Medical.API.Models.DTOs;
using Medical.API.Attributes;

namespace Medical.API.Controllers;

/// <summary>
/// 药品入库控制器
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
[Authorize(Roles = "Admin,SuperAdmin")]
public class DrugStockInsController : ControllerBase
{
    private readonly MedicalDbContext _context;
    private readonly ILogger<DrugStockInsController> _logger;

    public DrugStockInsController(MedicalDbContext context, ILogger<DrugStockInsController> logger)
    {
        _context = context;
        _logger = logger;
    }

    /// <summary>
    /// 获取入库单列表
    /// </summary>
    /// <param name="status">状态筛选（可选）</param>
    /// <param name="startDate">开始日期（可选）</param>
    /// <param name="endDate">结束日期（可选）</param>
    /// <param name="page">页码</param>
    /// <param name="pageSize">每页数量</param>
    /// <returns>入库单列表</returns>
    [HttpGet]
    [RequirePermission("drugstockin.view")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<ActionResult> GetDrugStockIns(
        [FromQuery] int? status = null,
        [FromQuery] DateTime? startDate = null,
        [FromQuery] DateTime? endDate = null,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10)
    {
        var query = _context.DrugStockInHeads
            .Include(h => h.Operator)
            .Include(h => h.Lines)
            .AsQueryable();

        if (status.HasValue)
        {
            query = query.Where(h => h.Status == status.Value);
        }

        if (startDate.HasValue)
        {
            query = query.Where(h => h.OperationTime >= startDate.Value);
        }

        if (endDate.HasValue)
        {
            query = query.Where(h => h.OperationTime <= endDate.Value);
        }

        var total = await query.CountAsync();
        var items = await query
            .OrderByDescending(h => h.OperationTime)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(h => new
            {
                h.Id,
                h.InvoiceNo,
                h.SupplierName,
                h.OperatorId,
                Operator = new { h.Operator.Username },
                h.OperationTime,
                h.TotalAmount,
                h.Status,
                h.Remarks,
                LineCount = h.Lines.Count,
                h.CreatedAt
            })
            .ToListAsync();

        return Ok(new { items, total, page, pageSize });
    }

    /// <summary>
    /// 根据ID获取入库单详情（包含明细）
    /// </summary>
    /// <param name="id">入库单ID</param>
    /// <returns>入库单详情</returns>
    [HttpGet("{id}")]
    [RequirePermission("drugstockin.view")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult> GetDrugStockInById(Guid id)
    {
        var stockIn = await _context.DrugStockInHeads
            .Include(h => h.Operator)
            .Include(h => h.Lines)
                .ThenInclude(l => l.Drug)
                    .ThenInclude(d => d.Category)
            .FirstOrDefaultAsync(h => h.Id == id);

        if (stockIn == null)
        {
            return NotFound(new { message = "入库单不存在" });
        }

        return Ok(stockIn);
    }

    /// <summary>
    /// 创建入库单（包含明细）
    /// </summary>
    /// <param name="dto">入库单信息</param>
    /// <returns>创建的入库单</returns>
    [HttpPost]
    [RequirePermission("drugstockin.create")]
    [ProducesResponseType(typeof(DrugStockInHead), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<DrugStockInHead>> CreateDrugStockIn([FromBody] CreateDrugStockInHeadDto dto)
    {
        using var transaction = await _context.Database.BeginTransactionAsync();
        try
        {
            ModelState.Clear();

            var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(currentUserId) || !Guid.TryParse(currentUserId, out var currentUserIdGuid))
            {
                return Unauthorized();
            }

            if (dto == null)
            {
                return BadRequest(new { message = "请求数据不能为空" });
            }

            if (string.IsNullOrWhiteSpace(dto.InvoiceNo))
            {
                return BadRequest(new { message = "入库单号不能为空" });
            }

            if (dto.Lines == null || dto.Lines.Count == 0)
            {
                return BadRequest(new { message = "入库明细不能为空" });
            }

            // 检查入库单号是否已存在
            var existingStockIn = await _context.DrugStockInHeads
                .FirstOrDefaultAsync(h => h.InvoiceNo == dto.InvoiceNo);
            if (existingStockIn != null)
            {
                return BadRequest(new { message = "入库单号已存在" });
            }

            // 创建入库单头（操作员使用当前登录用户）
            var newHead = new DrugStockInHead
            {
                Id = Guid.NewGuid(),
                InvoiceNo = dto.InvoiceNo,
                SupplierName = dto.SupplierName,
                OperatorId = currentUserIdGuid, // 使用当前登录用户
                OperationTime = dto.OperationTime,
                TotalAmount = dto.TotalAmount,
                Remarks = string.IsNullOrWhiteSpace(dto.Remarks) ? null : dto.Remarks,
                Status = dto.Status,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            _context.DrugStockInHeads.Add(newHead);

            // 创建入库单明细并更新库存
            decimal totalAmount = 0;
            foreach (var lineDto in dto.Lines)
            {
                // 验证药品是否存在
                var drugExists = await _context.Drugs.AnyAsync(d => d.Id == lineDto.DrugId);
                if (!drugExists)
                {
                    return BadRequest(new { message = $"药品ID {lineDto.DrugId} 不存在" });
                }

                var subtotal = lineDto.Quantity * lineDto.PurchasePrice;
                totalAmount += subtotal;

                var newLine = new DrugStockInLine
                {
                    Id = Guid.NewGuid(),
                    HeadId = newHead.Id,
                    DrugId = lineDto.DrugId,
                    BatchNumber = lineDto.BatchNumber,
                    ProductionDate = lineDto.ProductionDate,
                    ExpiryDate = lineDto.ExpiryDate,
                    Quantity = lineDto.Quantity,
                    PurchasePrice = lineDto.PurchasePrice,
                    Subtotal = subtotal,
                    WarehouseLocation = lineDto.WarehouseLocation,
                    CreatedAt = DateTime.UtcNow
                };

                _context.DrugStockInLines.Add(newLine);

                // 更新或创建库存记录
                if (dto.Status == 1) // 已入库状态才更新库存
                {
                    var inventory = await _context.DrugInventories
                        .FirstOrDefaultAsync(i => i.DrugId == lineDto.DrugId && i.WarehouseLocation == lineDto.WarehouseLocation);

                    if (inventory == null)
                    {
                        inventory = new DrugInventory
                        {
                            Id = Guid.NewGuid(),
                            DrugId = lineDto.DrugId,
                            WarehouseLocation = lineDto.WarehouseLocation,
                            CurrentQuantity = lineDto.Quantity,
                            LastUpdatedAt = DateTime.UtcNow
                        };
                        _context.DrugInventories.Add(inventory);
                    }
                    else
                    {
                        inventory.CurrentQuantity += lineDto.Quantity;
                        inventory.LastUpdatedAt = DateTime.UtcNow;
                    }
                }
            }

            // 更新入库单总金额
            newHead.TotalAmount = totalAmount;

            await _context.SaveChangesAsync();
            await transaction.CommitAsync();

            // 重新加载包含明细的入库单
            var createdStockIn = await _context.DrugStockInHeads
                .Include(h => h.Operator)
                .Include(h => h.Lines)
                    .ThenInclude(l => l.Drug)
                .FirstOrDefaultAsync(h => h.Id == newHead.Id);

            _logger.LogInformation("药品入库单创建成功: Id={Id}, InvoiceNo={InvoiceNo}", 
                newHead.Id, newHead.InvoiceNo);

            return CreatedAtAction(nameof(GetDrugStockInById), new { id = newHead.Id }, createdStockIn);
        }
        catch (DbUpdateException dbEx)
        {
            await transaction.RollbackAsync();
            _logger.LogError(dbEx, "数据库更新失败");
            return BadRequest(new { message = "数据库操作失败", error = dbEx.InnerException?.Message ?? dbEx.Message });
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync();
            _logger.LogError(ex, "创建入库单失败");
            return BadRequest(new { message = "创建入库单失败", error = ex.Message });
        }
    }

    /// <summary>
    /// 取消入库单（回退库存）
    /// </summary>
    /// <param name="id">入库单ID</param>
    /// <returns>取消结果</returns>
    [HttpPost("{id}/cancel")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult> CancelDrugStockIn(Guid id)
    {
        using var transaction = await _context.Database.BeginTransactionAsync();
        try
        {
            var stockIn = await _context.DrugStockInHeads
                .Include(h => h.Lines)
                .FirstOrDefaultAsync(h => h.Id == id);

            if (stockIn == null)
            {
                return NotFound(new { message = "入库单不存在" });
            }

            if (stockIn.Status == 0)
            {
                return BadRequest(new { message = "入库单已取消" });
            }

            // 回退库存
            foreach (var line in stockIn.Lines)
            {
                var inventory = await _context.DrugInventories
                    .FirstOrDefaultAsync(i => i.DrugId == line.DrugId && i.WarehouseLocation == line.WarehouseLocation);

                if (inventory != null)
                {
                    inventory.CurrentQuantity -= line.Quantity;
                    if (inventory.CurrentQuantity < 0)
                    {
                        return BadRequest(new { message = $"药品 {line.Drug.CommonName} 库存不足，无法取消" });
                    }
                    inventory.LastUpdatedAt = DateTime.UtcNow;
                }
            }

            stockIn.Status = 0; // 已取消
            stockIn.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();
            await transaction.CommitAsync();

            _logger.LogInformation("入库单取消成功: Id={Id}", id);

            return Ok(new { message = "入库单已取消" });
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync();
            _logger.LogError(ex, "取消入库单失败");
            return BadRequest(new { message = "取消入库单失败", error = ex.Message });
        }
    }
}

