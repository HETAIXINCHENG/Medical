using Medical.API.Attributes;
using Medical.API.Data;
using Medical.API.Models.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Medical.API.Controllers;

/// <summary>
/// 财务应付管理
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class FinancialPayablesController : ControllerBase
{
    private readonly MedicalDbContext _context;
    private readonly ILogger<FinancialPayablesController> _logger;

    public FinancialPayablesController(MedicalDbContext context, ILogger<FinancialPayablesController> logger)
    {
        _context = context;
        _logger = logger;
    }

    [HttpGet]
    [RequirePermission("financial-payables.view")]
    public async Task<ActionResult> GetList(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10,
        [FromQuery] string? search = null,
        [FromQuery] string? status = null)
    {
        var query = _context.FinancialPayables.AsQueryable();

        if (!string.IsNullOrWhiteSpace(search))
        {
            query = query.Where(x =>
                (x.ReferenceNo != null && x.ReferenceNo.Contains(search)) ||
                (x.VendorName != null && x.VendorName.Contains(search)) ||
                (x.ExpenseType != null && x.ExpenseType.Contains(search)) ||
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
    [RequirePermission("financial-payables.view")]
    public async Task<ActionResult<FinancialPayable>> GetById(Guid id)
    {
        var entity = await _context.FinancialPayables.FindAsync(id);
        if (entity == null) return NotFound();
        return Ok(entity);
    }

    [HttpPost]
    [RequirePermission("financial-payables.create")]
    public async Task<ActionResult<FinancialPayable>> Create([FromBody] FinancialPayable input)
    {
        input.Id = Guid.NewGuid();
        input.CreatedAt = DateTime.UtcNow;
        input.UpdatedAt = DateTime.UtcNow;
        input.PendingAmount = input.Amount - input.PaidAmount;

        _context.FinancialPayables.Add(input);
        await _context.SaveChangesAsync();
        return Ok(input);
    }

    [HttpPut("{id}")]
    [RequirePermission("financial-payables.update")]
    public async Task<ActionResult<FinancialPayable>> Update(Guid id, [FromBody] FinancialPayable input)
    {
        var entity = await _context.FinancialPayables.FindAsync(id);
        if (entity == null) return NotFound();

        entity.OrderId = input.OrderId;
        entity.ReferenceNo = input.ReferenceNo;
        entity.VendorName = input.VendorName;
        entity.ExpenseType = input.ExpenseType;
        entity.Amount = input.Amount;
        entity.PaidAmount = input.PaidAmount;
        entity.PendingAmount = input.Amount - input.PaidAmount;
        entity.Status = input.Status;
        entity.Currency = input.Currency;
        entity.Remark = input.Remark;
        entity.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();
        return Ok(entity);
    }

    [HttpDelete("{id}")]
    [RequirePermission("financial-payables.delete")]
    public async Task<IActionResult> Delete(Guid id)
    {
        var entity = await _context.FinancialPayables.FindAsync(id);
        if (entity == null) return NotFound();

        _context.FinancialPayables.Remove(entity);
        await _context.SaveChangesAsync();
        return NoContent();
    }
}

