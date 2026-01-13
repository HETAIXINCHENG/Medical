using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Medical.API.Attributes;
using Medical.API.Data;
using Medical.API.Models.Entities;

namespace Medical.API.Controllers;

/// <summary>
/// 商品分类
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
[Authorize(Roles = "Admin,SuperAdmin")]
public class ProductCategoriesController : ControllerBase
{
    private readonly MedicalDbContext _context;

    public ProductCategoriesController(MedicalDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    [RequirePermission("product-categories.view")]
    public async Task<ActionResult> GetList([FromQuery] int page = 1, [FromQuery] int pageSize = 20)
    {
        var query = _context.ProductCategories.AsQueryable();
        var total = await query.CountAsync();
        var items = await query
            .OrderBy(c => c.SortOrder)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();
        return Ok(new { items, total, page, pageSize });
    }

    [HttpGet("{id}")]
    [RequirePermission("product-categories.view")]
    public async Task<ActionResult<ProductCategory>> Get(Guid id)
    {
        var entity = await _context.ProductCategories.FindAsync(id);
        if (entity == null) return NotFound();
        return Ok(entity);
    }

    [HttpPost]
    [RequirePermission("product-categories.create")]
    public async Task<ActionResult<ProductCategory>> Create(ProductCategory input)
    {
        input.Id = Guid.NewGuid();
        input.CreatedAt = DateTime.UtcNow;
        input.UpdatedAt = DateTime.UtcNow;
        _context.ProductCategories.Add(input);
        await _context.SaveChangesAsync();
        return Ok(input);
    }

    [HttpPut("{id}")]
    [RequirePermission("product-categories.update")]
    public async Task<ActionResult> Update(Guid id, ProductCategory input)
    {
        var entity = await _context.ProductCategories.FindAsync(id);
        if (entity == null) return NotFound();

        entity.Name = input.Name;
        entity.Code = input.Code;
        entity.SortOrder = input.SortOrder;
        entity.IsEnabled = input.IsEnabled;
        entity.UpdatedAt = DateTime.UtcNow;

        _context.ProductCategories.Update(entity);
        await _context.SaveChangesAsync();
        return Ok(entity);
    }

    [HttpDelete("{id}")]
    [RequirePermission("product-categories.delete")]
    public async Task<ActionResult> Delete(Guid id)
    {
        var entity = await _context.ProductCategories.FindAsync(id);
        if (entity == null) return NotFound();
        _context.ProductCategories.Remove(entity);
        await _context.SaveChangesAsync();
        return NoContent();
    }
}

