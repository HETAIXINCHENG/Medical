using Medical.API.Attributes;
using Medical.API.Data;
using Medical.API.Models.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Medical.API.Controllers;

/// <summary>
/// 财务费用管理
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class FinancialFeesController : ControllerBase
{
    private readonly MedicalDbContext _context;
    private readonly ILogger<FinancialFeesController> _logger;

    public FinancialFeesController(MedicalDbContext context, ILogger<FinancialFeesController> logger)
    {
        _context = context;
        _logger = logger;
    }

    [HttpGet]
    [RequirePermission("financial-fees.view")]
    public async Task<ActionResult> GetList(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10,
        [FromQuery] string? search = null,
        [FromQuery] string? feeType = null)
    {
        var query = _context.FinancialFees.AsQueryable();

        if (!string.IsNullOrWhiteSpace(search))
        {
            query = query.Where(x =>
                (x.ReferenceNo != null && x.ReferenceNo.Contains(search)) ||
                (x.FeeType != null && x.FeeType.Contains(search)) ||
                (x.Remark != null && x.Remark.Contains(search)));
        }

        if (!string.IsNullOrWhiteSpace(feeType))
        {
            query = query.Where(x => x.FeeType == feeType);
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
    [RequirePermission("financial-fees.view")]
    public async Task<ActionResult<FinancialFee>> GetById(Guid id)
    {
        var entity = await _context.FinancialFees.FindAsync(id);
        if (entity == null) return NotFound();
        return Ok(entity);
    }

    [HttpPost]
    [RequirePermission("financial-fees.create")]
    public async Task<ActionResult<FinancialFee>> Create([FromBody] FinancialFee input)
    {
        input.Id = Guid.NewGuid();
        input.CreatedAt = DateTime.UtcNow;
        input.UpdatedAt = DateTime.UtcNow;

        _context.FinancialFees.Add(input);
        await _context.SaveChangesAsync();
        return Ok(input);
    }

    [HttpPut("{id}")]
    [RequirePermission("financial-fees.update")]
    public async Task<ActionResult<FinancialFee>> Update(Guid id, [FromBody] FinancialFee input)
    {
        var entity = await _context.FinancialFees.FindAsync(id);
        if (entity == null) return NotFound();

        entity.OrderId = input.OrderId;
        entity.ReferenceNo = input.ReferenceNo;
        entity.FeeType = input.FeeType;
        entity.Amount = input.Amount;
        entity.Remark = input.Remark;
        entity.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();
        return Ok(entity);
    }

    [HttpDelete("{id}")]
    [RequirePermission("financial-fees.delete")]
    public async Task<IActionResult> Delete(Guid id)
    {
        var entity = await _context.FinancialFees.FindAsync(id);
        if (entity == null) return NotFound();

        _context.FinancialFees.Remove(entity);
        await _context.SaveChangesAsync();
        return NoContent();
    }
}

