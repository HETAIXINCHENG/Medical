using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Medical.API.Attributes;
using Medical.API.Data;
using Medical.API.Models.Entities;
using Medical.API.Models.DTOs;

namespace Medical.API.Controllers;

/// <summary>
/// 商品信息
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
[Authorize(Roles = "Admin,SuperAdmin")]
public class ProductsController : ControllerBase
{
    private readonly MedicalDbContext _context;
    private readonly ILogger<ProductsController> _logger;

    public ProductsController(MedicalDbContext context, ILogger<ProductsController> logger)
    {
        _context = context;
        _logger = logger;
    }

    [HttpGet]
    [AllowAnonymous] // 允许APP端匿名访问商品列表
    [RequirePermission("products.view")]
    public async Task<ActionResult> GetList([FromQuery] Guid? categoryId = null, [FromQuery] string? keyword = null, [FromQuery] int page = 1, [FromQuery] int pageSize = 20)
    {
        var query = _context.Products
            .Include(p => p.Category)
            .Include(p => p.Specs)
            .Where(p => p.IsEnabled) // 只返回启用的商品
            .AsQueryable();

        if (categoryId.HasValue)
        {
            query = query.Where(p => p.CategoryId == categoryId.Value);
        }

        if (!string.IsNullOrWhiteSpace(keyword))
        {
            query = query.Where(p => p.Name.Contains(keyword) || p.Code.Contains(keyword));
        }

        var total = await query.CountAsync();
        var items = await query
            .OrderBy(p => p.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return Ok(new { items, total, page, pageSize });
    }

    [HttpGet("{id}")]
    [RequirePermission("products.view")]
    public async Task<ActionResult> Get(Guid id)
    {
        var product = await _context.Products
            .Include(p => p.Category)
            .Include(p => p.Specs)
            .FirstOrDefaultAsync(p => p.Id == id);
        if (product == null) return NotFound();
        return Ok(product);
    }

    [HttpPost]
    [RequirePermission("products.create")]
    public async Task<ActionResult> Create([FromBody] CreateProductDto dto)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var product = new Product
        {
            Id = Guid.NewGuid(),
            CategoryId = dto.CategoryId,
            Name = dto.Name,
            Code = dto.Code,
            Description = dto.Description,
            Price = dto.Price,
            MarketPrice = dto.MarketPrice,
            IsEnabled = dto.IsEnabled,
            IsVirtual = dto.IsVirtual,
            CoverUrl = dto.CoverUrl,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        _context.Products.Add(product);
        await _context.SaveChangesAsync();
        
        // 重新加载以包含导航属性
        await _context.Entry(product).Reference(p => p.Category).LoadAsync();
        
        return Ok(product);
    }

    [HttpPut("{id}")]
    [RequirePermission("products.update")]
    public async Task<ActionResult> Update(Guid id, [FromBody] UpdateProductDto dto)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var entity = await _context.Products.FindAsync(id);
        if (entity == null) return NotFound();

        entity.CategoryId = dto.CategoryId;
        entity.Name = dto.Name;
        entity.Code = dto.Code;
        entity.Description = dto.Description;
        entity.Price = dto.Price;
        entity.MarketPrice = dto.MarketPrice;
        entity.IsEnabled = dto.IsEnabled;
        entity.IsVirtual = dto.IsVirtual;
        entity.CoverUrl = dto.CoverUrl;
        entity.UpdatedAt = DateTime.UtcNow;

        _context.Products.Update(entity);
        await _context.SaveChangesAsync();
        
        // 重新加载以包含导航属性
        await _context.Entry(entity).Reference(p => p.Category).LoadAsync();
        
        return Ok(entity);
    }

    [HttpDelete("{id}")]
    [RequirePermission("products.delete")]
    public async Task<ActionResult> Delete(Guid id)
    {
        var entity = await _context.Products.FindAsync(id);
        if (entity == null) return NotFound();
        _context.Products.Remove(entity);
        await _context.SaveChangesAsync();
        return NoContent();
    }
}

