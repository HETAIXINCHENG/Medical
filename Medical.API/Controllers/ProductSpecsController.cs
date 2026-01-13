using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Medical.API.Attributes;
using Medical.API.Data;
using Medical.API.Models.Entities;

namespace Medical.API.Controllers;

/// <summary>
/// 商品规格
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
[Authorize(Roles = "Admin,SuperAdmin")]
public class ProductSpecsController : ControllerBase
{
    private readonly MedicalDbContext _context;

    public ProductSpecsController(MedicalDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    [RequirePermission("product-specs.view")]
    public async Task<ActionResult> GetList([FromQuery] Guid? productId = null)
    {
        var query = _context.ProductSpecs.AsQueryable();
        if (productId.HasValue)
        {
            query = query.Where(s => s.ProductId == productId.Value);
        }

        var items = await query
            .OrderBy(s => s.CreatedAt)
            .ToListAsync();

        return Ok(new { items, total = items.Count });
    }

    [HttpPost]
    [RequirePermission("product-specs.create")]
    public async Task<ActionResult> Create([FromBody] ProductSpec input)
    {
        input.Id = Guid.NewGuid();
        input.CreatedAt = DateTime.UtcNow;
        input.UpdatedAt = DateTime.UtcNow;
        _context.ProductSpecs.Add(input);
        await _context.SaveChangesAsync();
        return Ok(input);
    }

    [HttpPut("{id}")]
    [RequirePermission("product-specs.update")]
    public async Task<ActionResult> Update(Guid id, [FromBody] ProductSpec input)
    {
        var entity = await _context.ProductSpecs.FindAsync(id);
        if (entity == null) return NotFound();

        entity.SpecName = input.SpecName;
        entity.SpecCode = input.SpecCode;
        entity.Price = input.Price;
        entity.Stock = input.Stock;
        entity.Weight = input.Weight;
        entity.IsDefault = input.IsDefault;
        entity.UpdatedAt = DateTime.UtcNow;

        _context.ProductSpecs.Update(entity);
        await _context.SaveChangesAsync();
        return Ok(entity);
    }

    [HttpDelete("{id}")]
    [RequirePermission("product-specs.delete")]
    public async Task<ActionResult> Delete(Guid id)
    {
        var entity = await _context.ProductSpecs.FindAsync(id);
        if (entity == null) return NotFound();
        _context.ProductSpecs.Remove(entity);
        await _context.SaveChangesAsync();
        return NoContent();
    }
}

