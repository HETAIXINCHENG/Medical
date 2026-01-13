using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Medical.API.Attributes;
using Medical.API.Data;
using Medical.API.Models.Entities;

namespace Medical.API.Controllers;

/// <summary>
/// 物流信息（发货单）
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
[Authorize(Roles = "Admin,SuperAdmin")]
public class ShipmentsController : ControllerBase
{
    private readonly MedicalDbContext _context;

    public ShipmentsController(MedicalDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    [RequirePermission("shipments.view")]
    public async Task<ActionResult> GetList([FromQuery] Guid? orderId = null)
    {
        var query = _context.Shipments
            .Include(s => s.ShipCompany)
            .Include(s => s.Tracks)
            .AsQueryable();

        if (orderId.HasValue)
        {
            query = query.Where(s => s.OrderId == orderId.Value);
        }

        var items = await query
            .OrderByDescending(s => s.CreatedAt)
            .ToListAsync();

        return Ok(new { items, total = items.Count });
    }

    [HttpPost]
    [RequirePermission("shipments.create")]
    public async Task<ActionResult> Create([FromBody] Shipment input)
    {
        input.Id = Guid.NewGuid();
        input.CreatedAt = DateTime.UtcNow;
        input.UpdatedAt = DateTime.UtcNow;
        _context.Shipments.Add(input);
        await _context.SaveChangesAsync();
        return Ok(input);
    }

    [HttpPut("{id}")]
    [RequirePermission("shipments.update")]
    public async Task<ActionResult> Update(Guid id, [FromBody] Shipment input)
    {
        var entity = await _context.Shipments.FindAsync(id);
        if (entity == null) return NotFound();

        entity.ShipCompanyId = input.ShipCompanyId;
        entity.TrackingNo = input.TrackingNo;
        entity.PackageIndex = input.PackageIndex;
        entity.ShippedAt = input.ShippedAt;
        entity.DeliveredAt = input.DeliveredAt;
        entity.Status = input.Status;
        entity.UpdatedAt = DateTime.UtcNow;

        _context.Shipments.Update(entity);
        await _context.SaveChangesAsync();
        return Ok(entity);
    }

    [HttpDelete("{id}")]
    [RequirePermission("shipments.delete")]
    public async Task<ActionResult> Delete(Guid id)
    {
        var entity = await _context.Shipments.FindAsync(id);
        if (entity == null) return NotFound();
        _context.Shipments.Remove(entity);
        await _context.SaveChangesAsync();
        return NoContent();
    }
}

