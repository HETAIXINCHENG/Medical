using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Medical.API.Attributes;
using Medical.API.Data;
using Medical.API.Models.Entities;

namespace Medical.API.Controllers;

/// <summary>
/// 订单信息
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
[Authorize(Roles = "Admin,SuperAdmin")]
public class OrdersController : ControllerBase
{
    private readonly MedicalDbContext _context;
    private readonly ILogger<OrdersController> _logger;

    public OrdersController(MedicalDbContext context, ILogger<OrdersController> logger)
    {
        _context = context;
        _logger = logger;
    }

    [HttpGet]
    [RequirePermission("orders.view")]
    public async Task<ActionResult> GetList([FromQuery] Guid? userId = null, [FromQuery] string? keyword = null, [FromQuery] int page = 1, [FromQuery] int pageSize = 20)
    {
        var query = _context.Orders
            .Include(o => o.Items)
            .Include(o => o.Payments)
            .Include(o => o.Shipments)
                .ThenInclude(s => s.ShipCompany)
            .AsQueryable();

        if (userId.HasValue)
        {
            query = query.Where(o => o.UserId == userId.Value);
        }

        if (!string.IsNullOrWhiteSpace(keyword))
        {
            query = query.Where(o => o.OrderNo.Contains(keyword) || (o.Phone != null && o.Phone.Contains(keyword)));
        }

        var total = await query.CountAsync();
        var items = await query
            .OrderByDescending(o => o.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return Ok(new { items, total, page, pageSize });
    }

    [HttpGet("{id}")]
    [RequirePermission("orders.view")]
    public async Task<ActionResult> Get(Guid id)
    {
        var order = await _context.Orders
            .Include(o => o.Items)
            .ThenInclude(i => i.Product)
            .Include(o => o.Items)
            .ThenInclude(i => i.ProductSpec)
            .Include(o => o.Payments)
            .Include(o => o.Shipments)
                .ThenInclude(s => s.ShipCompany)
            .Include(o => o.Shipments)
                .ThenInclude(s => s.Tracks)
            .Include(o => o.Refunds)
            .FirstOrDefaultAsync(o => o.Id == id);

        if (order == null) return NotFound();
        return Ok(order);
    }

    [HttpPost]
    [RequirePermission("orders.create")]
    public async Task<ActionResult> Create([FromBody] Order input)
    {
        input.Id = Guid.NewGuid();
        input.CreatedAt = DateTime.UtcNow;
        input.UpdatedAt = DateTime.UtcNow;
        _context.Orders.Add(input);
        await _context.SaveChangesAsync();
        return Ok(input);
    }

    [HttpPut("{id}")]
    [RequirePermission("orders.update")]
    public async Task<ActionResult> Update(Guid id, [FromBody] Order input)
    {
        var entity = await _context.Orders.FindAsync(id);
        if (entity == null) return NotFound();

        entity.Status = input.Status;
        entity.PayStatus = input.PayStatus;
        entity.PayMethod = input.PayMethod;
        entity.TotalAmount = input.TotalAmount;
        entity.PayAmount = input.PayAmount;
        entity.Consignee = input.Consignee;
        entity.Phone = input.Phone;
        entity.Province = input.Province;
        entity.City = input.City;
        entity.District = input.District;
        entity.AddressLine = input.AddressLine;
        entity.IsDropship = input.IsDropship;
        entity.DropshipVendor = input.DropshipVendor;
        entity.Remark = input.Remark;
        entity.UpdatedAt = DateTime.UtcNow;

        _context.Orders.Update(entity);
        await _context.SaveChangesAsync();
        return Ok(entity);
    }

    [HttpDelete("{id}")]
    [RequirePermission("orders.delete")]
    public async Task<ActionResult> Delete(Guid id)
    {
        var entity = await _context.Orders.FindAsync(id);
        if (entity == null) return NotFound();
        _context.Orders.Remove(entity);
        await _context.SaveChangesAsync();
        return NoContent();
    }
}

