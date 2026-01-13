using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Medical.API.Attributes;
using Medical.API.Data;
using Medical.API.Models.Entities;

namespace Medical.API.Controllers;

/// <summary>
/// 订单明细
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
[Authorize(Roles = "Admin,SuperAdmin")]
public class OrderItemsController : ControllerBase
{
    private readonly MedicalDbContext _context;

    public OrderItemsController(MedicalDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    [RequirePermission("order-items.view")]
    public async Task<ActionResult> GetList([FromQuery] Guid? orderId = null)
    {
        var query = _context.OrderItems.AsQueryable();
        if (orderId.HasValue)
        {
            query = query.Where(oi => oi.OrderId == orderId.Value);
        }
        var items = await query.ToListAsync();
        return Ok(new { items, total = items.Count });
    }
}

