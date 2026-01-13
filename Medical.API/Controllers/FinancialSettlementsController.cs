using Medical.API.Attributes;
using Medical.API.Data;
using Medical.API.Models.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Medical.API.Controllers;

/// <summary>
/// 财务结算管理
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class FinancialSettlementsController : ControllerBase
{
    private readonly MedicalDbContext _context;
    private readonly ILogger<FinancialSettlementsController> _logger;

    public FinancialSettlementsController(MedicalDbContext context, ILogger<FinancialSettlementsController> logger)
    {
        _context = context;
        _logger = logger;
    }

    [HttpGet]
    [RequirePermission("financial-settlements.view")]
    public async Task<ActionResult> GetList(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10,
        [FromQuery] string? search = null,
        [FromQuery] string? status = null)
    {
        var query = _context.FinancialSettlements.AsQueryable();

        if (!string.IsNullOrWhiteSpace(search))
        {
            query = query.Where(x =>
                x.Name.Contains(search) ||
                (x.Remark != null && x.Remark.Contains(search)));
        }

        if (!string.IsNullOrWhiteSpace(status))
        {
            query = query.Where(x => x.Status == status);
        }

        var total = await query.CountAsync();
        var items = await query
            .OrderByDescending(x => x.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return Ok(new { items, total, page, pageSize });
    }

    [HttpGet("{id}")]
    [RequirePermission("financial-settlements.view")]
    public async Task<ActionResult<FinancialSettlement>> GetById(Guid id)
    {
        var entity = await _context.FinancialSettlements.FindAsync(id);
        if (entity == null) return NotFound();
        return Ok(entity);
    }

    [HttpPost]
    [RequirePermission("financial-settlements.create")]
    public async Task<ActionResult<FinancialSettlement>> Create([FromBody] FinancialSettlement input)
    {
        input.Id = Guid.NewGuid();
        input.CreatedAt = DateTime.UtcNow;
        input.UpdatedAt = DateTime.UtcNow;

        _context.FinancialSettlements.Add(input);
        await _context.SaveChangesAsync();
        return Ok(input);
    }

    [HttpPut("{id}")]
    [RequirePermission("financial-settlements.update")]
    public async Task<ActionResult<FinancialSettlement>> Update(Guid id, [FromBody] FinancialSettlement input)
    {
        var entity = await _context.FinancialSettlements.FindAsync(id);
        if (entity == null) return NotFound();

        entity.Name = input.Name;
        entity.PeriodStart = input.PeriodStart;
        entity.PeriodEnd = input.PeriodEnd;
        entity.TotalReceivable = input.TotalReceivable;
        entity.TotalPayable = input.TotalPayable;
        entity.NetAmount = input.NetAmount;
        entity.Status = input.Status;
        entity.Remark = input.Remark;
        entity.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();
        return Ok(entity);
    }

    [HttpDelete("{id}")]
    [RequirePermission("financial-settlements.delete")]
    public async Task<IActionResult> Delete(Guid id)
    {
        var entity = await _context.FinancialSettlements.FindAsync(id);
        if (entity == null) return NotFound();

        _context.FinancialSettlements.Remove(entity);
        await _context.SaveChangesAsync();
        return NoContent();
    }
}

