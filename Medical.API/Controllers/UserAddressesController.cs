using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Medical.API.Attributes;
using Medical.API.Data;
using Medical.API.Models.Entities;

namespace Medical.API.Controllers;

/// <summary>
/// 收货地址
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
[Authorize(Roles = "Admin,SuperAdmin")]
public class UserAddressesController : ControllerBase
{
    private readonly MedicalDbContext _context;

    public UserAddressesController(MedicalDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    [RequirePermission("user-addresses.view")]
    public async Task<ActionResult> GetList([FromQuery] Guid? userId = null)
    {
        var query = _context.UserAddresses.AsQueryable();
        if (userId.HasValue)
        {
            query = query.Where(a => a.UserId == userId.Value);
        }
        var items = await query.OrderByDescending(a => a.IsDefault).ThenBy(a => a.CreatedAt).ToListAsync();
        return Ok(new { items, total = items.Count });
    }

    [HttpPost]
    [RequirePermission("user-addresses.create")]
    public async Task<ActionResult> Create(UserAddress input)
    {
        input.Id = Guid.NewGuid();
        input.CreatedAt = DateTime.UtcNow;
        input.UpdatedAt = DateTime.UtcNow;
        _context.UserAddresses.Add(input);
        await _context.SaveChangesAsync();
        return Ok(input);
    }

    [HttpPut("{id}")]
    [RequirePermission("user-addresses.update")]
    public async Task<ActionResult> Update(Guid id, UserAddress input)
    {
        var entity = await _context.UserAddresses.FindAsync(id);
        if (entity == null) return NotFound();

        entity.Consignee = input.Consignee;
        entity.Phone = input.Phone;
        entity.Province = input.Province;
        entity.City = input.City;
        entity.District = input.District;
        entity.AddressLine = input.AddressLine;
        entity.IsDefault = input.IsDefault;
        entity.UpdatedAt = DateTime.UtcNow;

        _context.UserAddresses.Update(entity);
        await _context.SaveChangesAsync();
        return Ok(entity);
    }

    [HttpDelete("{id}")]
    [RequirePermission("user-addresses.delete")]
    public async Task<ActionResult> Delete(Guid id)
    {
        var entity = await _context.UserAddresses.FindAsync(id);
        if (entity == null) return NotFound();
        _context.UserAddresses.Remove(entity);
        await _context.SaveChangesAsync();
        return NoContent();
    }
}

