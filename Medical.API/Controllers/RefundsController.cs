using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Medical.API.Attributes;
using Medical.API.Data;
using Medical.API.Models.Entities;

namespace Medical.API.Controllers;

/// <summary>
/// 退款记录
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
[Authorize(Roles = "Admin,SuperAdmin")]
public class RefundsController : ControllerBase
{
    private readonly MedicalDbContext _context;

    public RefundsController(MedicalDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    [RequirePermission("refunds.view")]
    public async Task<ActionResult> GetList([FromQuery] Guid? orderId = null)
    {
        var query = _context.Refunds.AsQueryable();
        if (orderId.HasValue)
        {
            query = query.Where(r => r.OrderId == orderId.Value);
        }
        var items = await query.OrderByDescending(r => r.CreatedAt).ToListAsync();
        return Ok(new { items, total = items.Count });
    }

    [HttpPost]
    [RequirePermission("refunds.create")]
    public async Task<ActionResult> Create([FromBody] Refund input)
    {
        input.Id = Guid.NewGuid();
        input.CreatedAt = DateTime.UtcNow;
        input.UpdatedAt = DateTime.UtcNow;
        input.InitiatedAt = input.InitiatedAt == default ? DateTime.UtcNow : input.InitiatedAt;
        _context.Refunds.Add(input);
        await _context.SaveChangesAsync();
        return Ok(input);
    }

    [HttpPut("{id}")]
    [RequirePermission("refunds.update")]
    public async Task<ActionResult> Update(Guid id, [FromBody] Refund input)
    {
        var entity = await _context.Refunds.FindAsync(id);
        if (entity == null) return NotFound();

        entity.Status = input.Status;
        entity.RefundMethod = input.RefundMethod;
        entity.ChannelRefundNo = input.ChannelRefundNo;
        entity.CompletedAt = input.CompletedAt;
        entity.UpdatedAt = DateTime.UtcNow;

        _context.Refunds.Update(entity);
        await _context.SaveChangesAsync();
        return Ok(entity);
    }
}

