using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Medical.API.Attributes;
using Medical.API.Data;
using Medical.API.Models.Entities;
using Medical.API.Services;

namespace Medical.API.Controllers;

/// <summary>
/// 购物车
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
[Authorize(Roles = "Admin,SuperAdmin")]
public class CartsController : ControllerBase
{
    private readonly MedicalDbContext _context;

    public CartsController(MedicalDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    [RequirePermission("carts.view")]
    public async Task<ActionResult> GetList([FromQuery] Guid? userId = null)
    {
        var query = _context.Carts
            .Include(c => c.Items)
            .ThenInclude(i => i.Product)
            .Include(c => c.Items)
            .ThenInclude(i => i.ProductSpec)
            .AsQueryable();

        if (userId.HasValue)
        {
            query = query.Where(c => c.UserId == userId.Value);
        }

        var carts = await query
            .Where(c => c.Items.Any()) // 过滤掉没有明细的空购物车，避免前端出现空行
            .ToListAsync();
        
        // 获取所有相关的用户ID
        var userIds = carts.Select(c => c.UserId).Distinct().ToList();
        
        // 加载用户信息
        var users = await _context.Users
            .Where(u => userIds.Contains(u.Id))
            .ToListAsync();
        
        // 加载患者信息（用于获取真实姓名）
        var patients = await _context.Patients
            .Where(p => userIds.Contains(p.UserId))
            .ToListAsync();
        
        // 构建返回数据，包含用户信息
        var items = carts.Select(cart =>
        {
            var user = users.FirstOrDefault(u => u.Id == cart.UserId);
            var patient = patients.FirstOrDefault(p => p.UserId == cart.UserId);

            var realNameCipher = patient?.RealName ?? string.Empty;
            var phoneCipher = !string.IsNullOrEmpty(patient?.PhoneNumber)
                ? patient!.PhoneNumber!
                : user?.PhoneNumber ?? string.Empty;

            var realName = realNameCipher ?? string.Empty;
            var phoneNumber = phoneCipher ?? string.Empty;
            
            return new
            {
                cart.Id,
                cart.UserId,
                Username = user?.Username ?? string.Empty,
                RealName = realName,
                PhoneNumber = phoneNumber,
                cart.Items,
                cart.CreatedAt,
                cart.UpdatedAt
            };
        }).ToList();
        
        return Ok(new { items, total = items.Count });
    }

    [HttpGet("tree")]
    [RequirePermission("carts.view")]
    public async Task<ActionResult> GetTree([FromQuery] Guid? userId = null)
    {
        var query = _context.Carts
            .Include(c => c.Items)
                .ThenInclude(i => i.Product)
            .Include(c => c.Items)
                .ThenInclude(i => i.ProductSpec)
            .AsQueryable();

        if (userId.HasValue)
        {
            query = query.Where(c => c.UserId == userId.Value);
        }

        var carts = await query.ToListAsync();

        // 获取所有相关的用户ID
        var userIds = carts.Select(c => c.UserId).Distinct().ToList();

        // 加载用户信息
        var users = await _context.Users
            .Where(u => userIds.Contains(u.Id))
            .ToListAsync();

        // 加载患者信息（用于获取真实姓名）
        var patients = await _context.Patients
            .Where(p => userIds.Contains(p.UserId))
            .ToListAsync();

        var items = carts.Select(cart =>
        {
            var user = users.FirstOrDefault(u => u.Id == cart.UserId);
            var patient = patients.FirstOrDefault(p => p.UserId == cart.UserId);

            var realNameCipher = patient?.RealName ?? string.Empty;
            var phoneCipher = !string.IsNullOrEmpty(patient?.PhoneNumber)
                ? patient!.PhoneNumber!
                : user?.PhoneNumber ?? string.Empty;

            var realName = realNameCipher ?? string.Empty;
            var phoneNumber = phoneCipher ?? string.Empty;

            var children = cart.Items.Select(item => new
            {
                id = item.Id,
                productName = item.Product?.Name ?? string.Empty,
                specName = item.ProductSpec?.SpecName ?? string.Empty,
                quantity = item.Quantity,
                price = item.Price,
                subtotal = item.Price * item.Quantity
            }).ToList();

            return new
            {
                id = cart.Id,
                userId = cart.UserId,
                username = user?.Username ?? string.Empty,
                realName,
                phoneNumber,
                itemCount = cart.Items.Count,
                totalAmount = cart.Items.Sum(i => i.Price * i.Quantity),
                createdAt = cart.CreatedAt,
                updatedAt = cart.UpdatedAt,
                children
            };
        }).ToList();

        return Ok(new { items, total = items.Count });
    }

    [HttpPost]
    [RequirePermission("carts.create")]
    public async Task<ActionResult> Create([FromBody] Cart input)
    {
        input.Id = Guid.NewGuid();
        input.CreatedAt = DateTime.UtcNow;
        input.UpdatedAt = DateTime.UtcNow;
        _context.Carts.Add(input);
        await _context.SaveChangesAsync();
        return Ok(input);
    }

    [HttpDelete("{id}")]
    [RequirePermission("carts.delete")]
    public async Task<ActionResult> Delete(Guid id)
    {
        var cart = await _context.Carts
            .Include(c => c.Items)
            .FirstOrDefaultAsync(c => c.Id == id);
        
        if (cart == null) return NotFound();
        
        // 删除购物车及其所有商品项（级联删除）
        _context.Carts.Remove(cart);
        await _context.SaveChangesAsync();
        
        return NoContent();
    }

    /// <summary>
    /// 按用户ID删除该用户的所有购物车及其商品项
    /// </summary>
    [HttpDelete("user/{userId}")]
    [RequirePermission("carts.delete")]
    public async Task<ActionResult> DeleteByUserId(Guid userId)
    {
        // 查找该用户的所有购物车
        var carts = await _context.Carts
            .Include(c => c.Items)
            .Where(c => c.UserId == userId)
            .ToListAsync();
        
        if (carts == null || carts.Count == 0)
        {
            return NotFound(new { message = "该用户没有购物车记录" });
        }
        
        // 删除所有购物车及其商品项（级联删除）
        _context.Carts.RemoveRange(carts);
        await _context.SaveChangesAsync();
        
        return Ok(new { message = $"已删除该用户的 {carts.Count} 个购物车记录" });
    }

    [HttpPost("{cartId}/items")]
    [RequirePermission("carts.update")]
    public async Task<ActionResult> AddItem(Guid cartId, [FromBody] CartItem input)
    {
        var cart = await _context.Carts.FindAsync(cartId);
        if (cart == null) return NotFound();
        input.Id = Guid.NewGuid();
        input.CartId = cartId;
        input.CreatedAt = DateTime.UtcNow;
        input.UpdatedAt = DateTime.UtcNow;
        _context.CartItems.Add(input);
        await _context.SaveChangesAsync();
        return Ok(input);
    }

    [HttpDelete("items/{itemId}")]
    [RequirePermission("carts.update")]
    public async Task<ActionResult> RemoveItem(Guid itemId)
    {
        var item = await _context.CartItems.FindAsync(itemId);
        if (item == null) return NotFound();
        _context.CartItems.Remove(item);
        await _context.SaveChangesAsync();
        return NoContent();
    }
}

