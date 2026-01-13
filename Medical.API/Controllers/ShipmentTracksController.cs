using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Medical.API.Attributes;
using Medical.API.Data;
using Medical.API.Models.Entities;

namespace Medical.API.Controllers;

/// <summary>
/// 物流轨迹
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
[Authorize(Roles = "Admin,SuperAdmin")]
public class ShipmentTracksController : ControllerBase
{
    private readonly MedicalDbContext _context;

    public ShipmentTracksController(MedicalDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    [RequirePermission("shipment-tracks.view")]
    public async Task<ActionResult> GetList([FromQuery] Guid? shipmentId = null)
    {
        var query = _context.ShipmentTracks.AsQueryable();
        if (shipmentId.HasValue)
        {
            query = query.Where(t => t.ShipmentId == shipmentId.Value);
        }
        var items = await query.OrderByDescending(t => t.OccurredAt).ToListAsync();
        return Ok(new { items, total = items.Count });
    }

    [HttpPost]
    [RequirePermission("shipment-tracks.create")]
    public async Task<ActionResult> Create([FromBody] ShipmentTrack input)
    {
        input.Id = Guid.NewGuid();
        input.OccurredAt = input.OccurredAt == default ? DateTime.UtcNow : input.OccurredAt;
        _context.ShipmentTracks.Add(input);
        await _context.SaveChangesAsync();
        return Ok(input);
    }

    [HttpDelete("{id}")]
    [RequirePermission("shipment-tracks.delete")]
    public async Task<ActionResult> Delete(Guid id)
    {
        var entity = await _context.ShipmentTracks.FindAsync(id);
        if (entity == null) return NotFound();
        _context.ShipmentTracks.Remove(entity);
        await _context.SaveChangesAsync();
        return NoContent();
    }
}

