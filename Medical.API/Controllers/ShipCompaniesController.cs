using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Medical.API.Attributes;
using Medical.API.Data;
using Medical.API.Models.Entities;

namespace Medical.API.Controllers;

/// <summary>
/// 物流公司枚举维护
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
[Authorize(Roles = "Admin,SuperAdmin")]
public class ShipCompaniesController : ControllerBase
{
    private readonly MedicalDbContext _context;

    public ShipCompaniesController(MedicalDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    [RequirePermission("ship-companies.view")]
    public async Task<ActionResult> GetList()
    {
        var items = await _context.ShipCompanies
            .OrderBy(c => c.Name)
            .ToListAsync();
        return Ok(new { items, total = items.Count });
    }

    [HttpPost]
    [RequirePermission("ship-companies.create")]
    public async Task<ActionResult> Create([FromBody] ShipCompany input)
    {
        input.Id = Guid.NewGuid();
        input.CreatedAt = DateTime.UtcNow;
        input.UpdatedAt = DateTime.UtcNow;
        _context.ShipCompanies.Add(input);
        await _context.SaveChangesAsync();
        return Ok(input);
    }

    [HttpPut("{id}")]
    [RequirePermission("ship-companies.update")]
    public async Task<ActionResult> Update(Guid id, [FromBody] ShipCompany input)
    {
        var entity = await _context.ShipCompanies.FindAsync(id);
        if (entity == null) return NotFound();
        entity.Name = input.Name;
        entity.Code = input.Code;
        entity.ContactUrl = input.ContactUrl;
        entity.Phone = input.Phone;
        entity.IsEnabled = input.IsEnabled;
        entity.UpdatedAt = DateTime.UtcNow;
        _context.ShipCompanies.Update(entity);
        await _context.SaveChangesAsync();
        return Ok(entity);
    }

    [HttpDelete("{id}")]
    [RequirePermission("ship-companies.delete")]
    public async Task<ActionResult> Delete(Guid id)
    {
        var entity = await _context.ShipCompanies.FindAsync(id);
        if (entity == null) return NotFound();
        _context.ShipCompanies.Remove(entity);
        await _context.SaveChangesAsync();
        return NoContent();
    }
}

