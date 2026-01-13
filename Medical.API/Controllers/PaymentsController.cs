using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Medical.API.Attributes;
using Medical.API.Data;
using Medical.API.Models.Entities;

namespace Medical.API.Controllers;

/// <summary>
/// 支付记录
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
[Authorize(Roles = "Admin,SuperAdmin")]
public class PaymentsController : ControllerBase
{
    private readonly MedicalDbContext _context;

    public PaymentsController(MedicalDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    [RequirePermission("payments.view")]
    public async Task<ActionResult> GetList([FromQuery] Guid? orderId = null)
    {
        var query = _context.Payments.AsQueryable();
        if (orderId.HasValue)
        {
            query = query.Where(p => p.OrderId == orderId.Value);
        }
        var items = await query.OrderByDescending(p => p.PayTime).ToListAsync();
        return Ok(new { items, total = items.Count });
    }

    [HttpPost]
    [RequirePermission("payments.create")]
    public async Task<ActionResult> Create([FromBody] Payment input)
    {
        input.Id = Guid.NewGuid();
        input.CreatedAt = DateTime.UtcNow;
        input.UpdatedAt = DateTime.UtcNow;
        _context.Payments.Add(input);
        await _context.SaveChangesAsync();
        return Ok(input);
    }
}

