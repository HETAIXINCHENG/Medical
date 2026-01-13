using Medical.API.Attributes;
using Medical.API.Data;
using Medical.API.Models.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Medical.API.Controllers;

/// <summary>
/// 财务应收管理
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class FinancialReceivablesController : ControllerBase
{
    private readonly MedicalDbContext _context;
    private readonly ILogger<FinancialReceivablesController> _logger;

    public FinancialReceivablesController(MedicalDbContext context, ILogger<FinancialReceivablesController> logger)
    {
        _context = context;
        _logger = logger;
    }

    [HttpGet]
    [RequirePermission("financial-receivables.view")]
    public async Task<ActionResult> GetList(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10,
        [FromQuery] string? search = null,
        [FromQuery] string? status = null)
    {
        var query = _context.FinancialReceivables.AsQueryable();

        if (!string.IsNullOrWhiteSpace(search))
        {
            query = query.Where(x =>
                (x.ReferenceNo != null && x.ReferenceNo.Contains(search)) ||
                (x.Channel != null && x.Channel.Contains(search)) ||
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
    [RequirePermission("financial-receivables.view")]
    public async Task<ActionResult<FinancialReceivable>> GetById(Guid id)
    {
        var entity = await _context.FinancialReceivables.FindAsync(id);
        if (entity == null) return NotFound();
        return Ok(entity);
    }

    [HttpPost]
    [RequirePermission("financial-receivables.create")]
    public async Task<ActionResult<FinancialReceivable>> Create([FromBody] FinancialReceivable input)
    {
        input.Id = Guid.NewGuid();
        input.CreatedAt = DateTime.UtcNow;
        input.UpdatedAt = DateTime.UtcNow;
        input.PendingAmount = input.Amount - input.ReceivedAmount;

        _context.FinancialReceivables.Add(input);
        await _context.SaveChangesAsync();
        return Ok(input);
    }

    [HttpPut("{id}")]
    [RequirePermission("financial-receivables.update")]
    public async Task<ActionResult<FinancialReceivable>> Update(Guid id, [FromBody] FinancialReceivable input)
    {
        var entity = await _context.FinancialReceivables.FindAsync(id);
        if (entity == null) return NotFound();

        entity.OrderId = input.OrderId;
        entity.ReferenceNo = input.ReferenceNo;
        entity.Channel = input.Channel;
        entity.Amount = input.Amount;
        entity.ReceivedAmount = input.ReceivedAmount;
        entity.PendingAmount = input.Amount - input.ReceivedAmount;
        entity.Status = input.Status;
        entity.Currency = input.Currency;
        entity.Remark = input.Remark;
        entity.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();
        return Ok(entity);
    }

    [HttpDelete("{id}")]
    [RequirePermission("financial-receivables.delete")]
    public async Task<IActionResult> Delete(Guid id)
    {
        var entity = await _context.FinancialReceivables.FindAsync(id);
        if (entity == null) return NotFound();

        _context.FinancialReceivables.Remove(entity);
        await _context.SaveChangesAsync();
        return NoContent();
    }
}

