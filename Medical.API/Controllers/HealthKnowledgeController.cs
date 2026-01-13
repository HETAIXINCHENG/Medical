using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Medical.API.Data;
using Medical.API.Models.Entities;
using Microsoft.AspNetCore.Authorization;
using Medical.API.Attributes;
using System.Security.Claims;
using System.Collections.Concurrent;

namespace Medical.API.Controllers;

/// <summary>
/// 健康知识控制器
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class HealthKnowledgeController : ControllerBase
{
    private readonly MedicalDbContext _context;
    private readonly ILogger<HealthKnowledgeController> _logger;
    
    // 用于防止重复增加阅读量的缓存（健康知识ID + IP地址 -> 时间戳）
    private static readonly ConcurrentDictionary<string, DateTime> _readCountCache = new();
    private static readonly TimeSpan _cacheExpiration = TimeSpan.FromSeconds(5); // 5秒内不重复增加
    private static readonly object _cacheLock = new object(); // 用于同步的锁对象

    public HealthKnowledgeController(MedicalDbContext context, ILogger<HealthKnowledgeController> logger)
    {
        _context = context;
        _logger = logger;
    }

    /// <summary>
    /// 获取健康知识列表
    /// </summary>
    /// <param name="category">分类</param>
    /// <param name="isHot">是否热门</param>
    /// <param name="isRecommended">是否推荐</param>
    /// <param name="keyword">关键词搜索（标题、摘要、内容）</param>
    /// <param name="page">页码</param>
    /// <param name="pageSize">每页数量</param>
    /// <returns>健康知识列表</returns>
    [HttpGet]
    [AllowAnonymous] // 允许匿名访问，首页需要显示推荐健康知识
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<ActionResult> GetHealthKnowledge(
        [FromQuery] string? category = null,
        [FromQuery] bool? isHot = null,
        [FromQuery] bool? isRecommended = null,
        [FromQuery] string? keyword = null,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10)
    {
        var query = _context.HealthKnowledge.AsQueryable();

        if (!string.IsNullOrEmpty(category))
        {
            query = query.Where(h => h.Category == category);
        }

        if (isHot.HasValue)
        {
            query = query.Where(h => h.IsHot == isHot.Value);
        }

        if (isRecommended.HasValue)
        {
            query = query.Where(h => h.IsRecommended == isRecommended.Value);
        }

        // 关键词搜索
        if (!string.IsNullOrEmpty(keyword))
        {
            query = query.Where(h => 
                h.Title.Contains(keyword) ||
                (h.Summary != null && h.Summary.Contains(keyword)) ||
                h.Content.Contains(keyword));
        }

        // 获取总数
        var total = await query.CountAsync();

        var knowledge = await query
            .OrderByDescending(h => h.ReadCount)
            .ThenByDescending(h => h.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        // 为每个健康知识关联医生信息
        var result = knowledge.Select(k => {
            // 如果Author字段是GUID格式，查询医生信息
            if (!string.IsNullOrEmpty(k.Author) && Guid.TryParse(k.Author, out var doctorId))
            {
                var doctor = _context.Doctors
                    .Include(d => d.Department)
                    .FirstOrDefault(d => d.Id == doctorId);
                
                if (doctor != null)
                {
                    return new {
                        k.Id,
                        k.Title,
                        k.Content,
                        k.Summary,
                        k.CoverImageUrl,
                        k.Author,
                        k.Source,
                        k.Category,
                        k.ReadCount,
                        k.FavoriteCount,
                        k.IsHot,
                        k.IsRecommended,
                        k.CreatedAt,
                        k.UpdatedAt,
                        AuthorName = doctor.Name,
                        AuthorTitle = doctor.Title,
                        AuthorAvatar = doctor.AvatarUrl,
                        AuthorHospital = doctor.Hospital,
                        AuthorDepartment = doctor.Department?.Name
                    };
                }
            }

            // 如果没有关联医生，返回原始数据
            return new {
                k.Id,
                k.Title,
                k.Content,
                k.Summary,
                k.CoverImageUrl,
                k.Author,
                k.Source,
                k.Category,
                k.ReadCount,
                k.FavoriteCount,
                k.IsHot,
                k.IsRecommended,
                k.CreatedAt,
                k.UpdatedAt,
                AuthorName = (string?)null,
                AuthorTitle = (string?)null,
                AuthorAvatar = (string?)null,
                AuthorHospital = (string?)null,
                AuthorDepartment = (string?)null
            };
        }).ToList();

        // 返回分页格式，兼容前端
        return Ok(new { items = result, total, page, pageSize });
    }

    /// <summary>
    /// 根据ID获取健康知识详情
    /// </summary>
    /// <param name="id">健康知识ID</param>
    /// <returns>健康知识详情</returns>
    [HttpGet("{id}")]
    [AllowAnonymous] // 允许匿名访问，患者端需要查看健康知识详情
    [ProducesResponseType(typeof(HealthKnowledge), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<HealthKnowledge>> GetHealthKnowledgeById(Guid id)
    {
        var knowledge = await _context.HealthKnowledge.FindAsync(id);

        if (knowledge == null)
        {
            return NotFound(new { message = "健康知识不存在" });
        }

        // 防止重复增加阅读量：使用缓存机制，在短时间内（5秒）同一个IP对同一篇文章只增加一次
        // 使用锁确保原子操作，防止并发请求同时增加
        var clientIp = HttpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown";
        var cacheKey = $"{id}_{clientIp}";
        var now = DateTime.UtcNow;

        bool shouldIncrement = false;
        Guid? authorDoctorId = null;
        
        // 使用锁确保检查-更新操作的原子性
        lock (_cacheLock)
        {
            // 检查缓存中是否有记录
            if (!_readCountCache.TryGetValue(cacheKey, out var lastReadTime) || 
                (now - lastReadTime) > _cacheExpiration)
            {
                // 如果缓存中没有记录，或者已经超过过期时间，则增加阅读量
                knowledge.ReadCount++;
                shouldIncrement = true;
                
                // 获取作者医生ID（在锁内获取，避免重复查询）
                if (!string.IsNullOrEmpty(knowledge.Author) && Guid.TryParse(knowledge.Author, out var parsedDoctorId))
                {
                    authorDoctorId = parsedDoctorId;
                }
                
                // 更新缓存（在锁内更新，确保原子性）
                _readCountCache.AddOrUpdate(cacheKey, now, (key, oldValue) => now);
            }
        }

        // 在锁外更新医生的总阅读数（如果需要）
        if (shouldIncrement && authorDoctorId.HasValue)
        {
            var authorDoctor = await _context.Doctors.FindAsync(authorDoctorId.Value);
            if (authorDoctor != null)
            {
                authorDoctor.TotalReadCount++;
                authorDoctor.UpdatedAt = DateTime.UtcNow;
            }
        }

        // 在锁外保存数据库更改，避免长时间持有锁
        await _context.SaveChangesAsync();
        
        // 定期清理过期的缓存项（避免内存泄漏）
        // 每100次请求清理一次，避免频繁清理影响性能
        if (_readCountCache.Count % 100 == 0)
        {
            CleanExpiredCacheEntries();
        }

        // 关联医生信息
        if (!string.IsNullOrEmpty(knowledge.Author) && Guid.TryParse(knowledge.Author, out var doctorId))
        {
            var doctor = await _context.Doctors
                .Include(d => d.Department)
                .FirstOrDefaultAsync(d => d.Id == doctorId);
            
            if (doctor != null)
            {
                return Ok(new {
                    knowledge.Id,
                    knowledge.Title,
                    knowledge.Content,
                    knowledge.Summary,
                    knowledge.CoverImageUrl,
                    knowledge.Author,
                    knowledge.Source,
                    knowledge.Category,
                    knowledge.ReadCount,
                    knowledge.FavoriteCount,
                    knowledge.IsHot,
                    knowledge.IsRecommended,
                    knowledge.CreatedAt,
                    knowledge.UpdatedAt,
                    AuthorName = doctor.Name,
                    AuthorTitle = doctor.Title,
                    AuthorAvatar = doctor.AvatarUrl,
                    AuthorHospital = doctor.Hospital,
                    AuthorDepartment = doctor.Department?.Name
                });
            }
        }

        // 如果没有关联医生，返回原始数据
        return Ok(new {
            knowledge.Id,
            knowledge.Title,
            knowledge.Content,
            knowledge.Summary,
            knowledge.CoverImageUrl,
            knowledge.Author,
            knowledge.Source,
            knowledge.Category,
            knowledge.ReadCount,
            knowledge.FavoriteCount,
            knowledge.IsHot,
            knowledge.IsRecommended,
            knowledge.CreatedAt,
            knowledge.UpdatedAt,
            AuthorName = (string?)null,
            AuthorTitle = (string?)null,
            AuthorAvatar = (string?)null,
            AuthorHospital = (string?)null,
            AuthorDepartment = (string?)null
        });
    }

    /// <summary>
    /// 搜索健康知识
    /// </summary>
    /// <param name="keyword">关键词</param>
    /// <param name="page">页码</param>
    /// <param name="pageSize">每页数量</param>
    /// <returns>健康知识列表</returns>
    [HttpGet("search")]
    [AllowAnonymous] // 允许匿名访问，患者端需要搜索健康知识
    [ProducesResponseType(typeof(List<HealthKnowledge>), StatusCodes.Status200OK)]
    public async Task<ActionResult<List<HealthKnowledge>>> SearchHealthKnowledge(
        [FromQuery] string keyword,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10)
    {
        if (string.IsNullOrWhiteSpace(keyword))
        {
            return BadRequest(new { message = "搜索关键词不能为空" });
        }

        var query = _context.HealthKnowledge
            .Where(h => h.Title.Contains(keyword) || 
                       (h.Summary != null && h.Summary.Contains(keyword)) ||
                       h.Content.Contains(keyword));

        // 获取总数
        var total = await query.CountAsync();

        var knowledge = await query
            .OrderByDescending(h => h.ReadCount)
            .ThenByDescending(h => h.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        // 为每个健康知识关联医生信息
        var result = knowledge.Select(k => {
            // 如果Author字段是GUID格式，查询医生信息
            if (!string.IsNullOrEmpty(k.Author) && Guid.TryParse(k.Author, out var doctorId))
            {
                var doctor = _context.Doctors
                    .Include(d => d.Department)
                    .FirstOrDefault(d => d.Id == doctorId);
                
                if (doctor != null)
                {
                    return new {
                        k.Id,
                        k.Title,
                        k.Content,
                        k.Summary,
                        k.CoverImageUrl,
                        k.Author,
                        k.Source,
                        k.Category,
                        k.ReadCount,
                        k.FavoriteCount,
                        k.IsHot,
                        k.IsRecommended,
                        k.CreatedAt,
                        k.UpdatedAt,
                        AuthorName = doctor.Name,
                        AuthorTitle = doctor.Title,
                        AuthorAvatar = doctor.AvatarUrl,
                        AuthorHospital = doctor.Hospital,
                        AuthorDepartment = doctor.Department?.Name
                    };
                }
            }

            // 如果没有关联医生，返回原始数据
            return new {
                k.Id,
                k.Title,
                k.Content,
                k.Summary,
                k.CoverImageUrl,
                k.Author,
                k.Source,
                k.Category,
                k.ReadCount,
                k.FavoriteCount,
                k.IsHot,
                k.IsRecommended,
                k.CreatedAt,
                k.UpdatedAt,
                AuthorName = (string?)null,
                AuthorTitle = (string?)null,
                AuthorAvatar = (string?)null,
                AuthorHospital = (string?)null,
                AuthorDepartment = (string?)null
            };
        }).ToList();

        // 返回分页格式，兼容前端
        return Ok(new { items = result, total, page, pageSize });
    }

    /// <summary>
    /// 创建健康知识
    /// </summary>
    /// <param name="knowledge">健康知识信息</param>
    /// <returns>创建的健康知识</returns>
    [HttpPost]
    [RequirePermission("healthknowledge.create")]
    [Authorize(Roles = "Admin,SuperAdmin")]
    [ProducesResponseType(typeof(HealthKnowledge), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<HealthKnowledge>> CreateHealthKnowledge([FromBody] HealthKnowledge knowledge)
    {
        if (string.IsNullOrWhiteSpace(knowledge.Title))
        {
            return BadRequest(new { message = "标题不能为空" });
        }

        if (string.IsNullOrWhiteSpace(knowledge.Content))
        {
            return BadRequest(new { message = "内容不能为空" });
        }

        knowledge.Id = Guid.NewGuid();
        knowledge.CreatedAt = DateTime.UtcNow;
        knowledge.UpdatedAt = DateTime.UtcNow;
        knowledge.ReadCount = 0;
        knowledge.FavoriteCount = 0;

        // 确保空字符串转换为 null（对于可选字段）
        if (string.IsNullOrWhiteSpace(knowledge.CoverImageUrl))
        {
            knowledge.CoverImageUrl = null;
        }
        if (string.IsNullOrWhiteSpace(knowledge.Summary))
        {
            knowledge.Summary = null;
        }
        if (string.IsNullOrWhiteSpace(knowledge.Author))
        {
            knowledge.Author = null;
        }
        if (string.IsNullOrWhiteSpace(knowledge.Source))
        {
            knowledge.Source = null;
        }
        if (string.IsNullOrWhiteSpace(knowledge.Category))
        {
            knowledge.Category = null;
        }

        _context.HealthKnowledge.Add(knowledge);
        await _context.SaveChangesAsync();

        return CreatedAtAction(nameof(GetHealthKnowledgeById), new { id = knowledge.Id }, knowledge);
    }

    /// <summary>
    /// 更新健康知识
    /// </summary>
    /// <param name="id">健康知识ID</param>
    /// <param name="knowledge">健康知识信息</param>
    /// <returns>更新后的健康知识</returns>
    [HttpPut("{id}")]
    [RequirePermission("healthknowledge.update")]
    [Authorize(Roles = "Admin,SuperAdmin")]
    [ProducesResponseType(typeof(HealthKnowledge), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<HealthKnowledge>> UpdateHealthKnowledge(Guid id, [FromBody] HealthKnowledge knowledge)
    {
        var existingKnowledge = await _context.HealthKnowledge.FindAsync(id);
        if (existingKnowledge == null)
        {
            return NotFound(new { message = "健康知识不存在" });
        }

        existingKnowledge.Title = knowledge.Title;
        existingKnowledge.Category = string.IsNullOrWhiteSpace(knowledge.Category) ? null : knowledge.Category;
        existingKnowledge.Author = string.IsNullOrWhiteSpace(knowledge.Author) ? null : knowledge.Author;
        existingKnowledge.Source = string.IsNullOrWhiteSpace(knowledge.Source) ? null : knowledge.Source;
        existingKnowledge.Summary = string.IsNullOrWhiteSpace(knowledge.Summary) ? null : knowledge.Summary;
        existingKnowledge.Content = knowledge.Content;
        existingKnowledge.CoverImageUrl = string.IsNullOrWhiteSpace(knowledge.CoverImageUrl) ? null : knowledge.CoverImageUrl;
        existingKnowledge.ReadCount = knowledge.ReadCount;
        existingKnowledge.FavoriteCount = knowledge.FavoriteCount;
        existingKnowledge.IsHot = knowledge.IsHot;
        existingKnowledge.IsRecommended = knowledge.IsRecommended;
        existingKnowledge.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        return Ok(existingKnowledge);
    }

    /// <summary>
    /// 删除健康知识
    /// </summary>
    /// <param name="id">健康知识ID</param>
    /// <returns>删除结果</returns>
    [HttpDelete("{id}")]
    [RequirePermission("healthknowledge.delete")]
    [Authorize(Roles = "Admin,SuperAdmin")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteHealthKnowledge(Guid id)
    {
        var knowledge = await _context.HealthKnowledge.FindAsync(id);
        if (knowledge == null)
        {
            return NotFound(new { message = "健康知识不存在" });
        }

        _context.HealthKnowledge.Remove(knowledge);
        await _context.SaveChangesAsync();

        return NoContent();
    }

    /// <summary>
    /// 收藏健康知识
    /// </summary>
    /// <param name="id">健康知识ID</param>
    /// <returns>收藏结果</returns>
    [HttpPost("{id}/favorite")]
    [Authorize] // 需要登录
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult> FavoriteHealthKnowledge(Guid id)
    {
        // 获取当前用户ID
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
        if (userIdClaim == null || !Guid.TryParse(userIdClaim.Value, out var userId))
        {
            return Unauthorized(new { message = "未登录或登录已过期" });
        }

        // 验证用户是否存在
        var userExists = await _context.Users.AnyAsync(u => u.Id == userId);
        if (!userExists)
        {
            return Unauthorized();
        }

        // 检查健康知识是否存在
        var knowledge = await _context.HealthKnowledge.FindAsync(id);
        if (knowledge == null)
        {
            return NotFound(new { message = "健康知识不存在" });
        }

        // 检查是否已经收藏
        var existingFavorite = await _context.UserHealthKnowledgeFavorites
            .FirstOrDefaultAsync(f => f.UserId == userId && f.HealthKnowledgeId == id);

        if (existingFavorite != null)
        {
            return BadRequest(new { message = "您已经收藏过这篇文章了" });
        }

        // 创建收藏记录
        var favorite = new UserHealthKnowledgeFavorite
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            HealthKnowledgeId = id,
            CreatedAt = DateTime.UtcNow
        };

        _context.UserHealthKnowledgeFavorites.Add(favorite);

        // 增加收藏数
        knowledge.FavoriteCount++;
        await _context.SaveChangesAsync();

        return Ok(new { message = "收藏成功", favoriteCount = knowledge.FavoriteCount });
    }

    /// <summary>
    /// 检查用户是否已收藏
    /// </summary>
    /// <param name="id">健康知识ID</param>
    /// <returns>是否已收藏</returns>
    [HttpGet("{id}/favorite/check")]
    [Authorize] // 需要登录
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<ActionResult> CheckFavorite(Guid id)
    {
        // 获取当前用户ID
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
        if (userIdClaim == null || !Guid.TryParse(userIdClaim.Value, out var userId))
        {
            return Ok(new { isFavorited = false });
        }

        // 验证用户是否存在
        var userExists = await _context.Users.AnyAsync(u => u.Id == userId);
        if (!userExists)
        {
            return Ok(new { isFavorited = false });
        }

        // 检查是否已收藏
        var isFavorited = await _context.UserHealthKnowledgeFavorites
            .AnyAsync(f => f.UserId == userId && f.HealthKnowledgeId == id);

        return Ok(new { isFavorited });
    }

    /// <summary>
    /// 获取当前用户收藏的健康知识列表
    /// </summary>
    /// <param name="page">页码</param>
    /// <param name="pageSize">每页数量</param>
    /// <returns>收藏的健康知识列表</returns>
    [HttpGet("my-favorites")]
    [Authorize] // 需要登录
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<ActionResult> GetMyFavorites(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10)
    {
        // 获取当前用户ID
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
        if (userIdClaim == null || !Guid.TryParse(userIdClaim.Value, out var userId))
        {
            return Unauthorized();
        }

        // 验证用户是否存在
        var userExists = await _context.Users.AnyAsync(u => u.Id == userId);
        if (!userExists)
        {
            return Unauthorized();
        }

        // 获取用户收藏的健康知识ID列表
        var favoriteIds = await _context.UserHealthKnowledgeFavorites
            .Where(f => f.UserId == userId)
            .OrderByDescending(f => f.CreatedAt)
            .Select(f => f.HealthKnowledgeId)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        var total = await _context.UserHealthKnowledgeFavorites
            .CountAsync(f => f.UserId == userId);

        if (favoriteIds.Count == 0)
        {
            return Ok(new { items = new List<object>(), total, page, pageSize });
        }

        // 获取健康知识详情
        var knowledge = await _context.HealthKnowledge
            .Where(k => favoriteIds.Contains(k.Id))
            .ToListAsync();

        // 按收藏时间排序（保持收藏顺序）
        var orderedKnowledge = favoriteIds
            .Select(id => knowledge.FirstOrDefault(k => k.Id == id))
            .Where(k => k != null)
            .ToList();

        // 为每个健康知识关联医生信息
        var result = orderedKnowledge.Select(k => {
            // 如果Author字段是GUID格式，查询医生信息
            if (!string.IsNullOrEmpty(k.Author) && Guid.TryParse(k.Author, out var doctorId))
            {
                var doctor = _context.Doctors
                    .Include(d => d.Department)
                    .FirstOrDefault(d => d.Id == doctorId);
                
                if (doctor != null)
                {
                    return new {
                        k.Id,
                        k.Title,
                        k.Content,
                        k.Summary,
                        k.CoverImageUrl,
                        k.Category,
                        k.IsHot,
                        k.IsRecommended,
                        k.ReadCount,
                        k.FavoriteCount,
                        k.CreatedAt,
                        k.UpdatedAt,
                        AuthorName = doctor.Name,
                        AuthorTitle = doctor.Title,
                        AuthorAvatar = doctor.AvatarUrl,
                        AuthorDepartment = doctor.Department?.Name,
                        AuthorHospital = doctor.Hospital
                    };
                }
            }
            
            // 如果没有关联医生，返回基本信息
            return new {
                k.Id,
                k.Title,
                k.Content,
                k.Summary,
                k.CoverImageUrl,
                k.Category,
                k.IsHot,
                k.IsRecommended,
                k.ReadCount,
                k.FavoriteCount,
                k.CreatedAt,
                k.UpdatedAt,
                AuthorName = (string?)null,
                AuthorTitle = (string?)null,
                AuthorAvatar = (string?)null,
                AuthorDepartment = (string?)null,
                AuthorHospital = (string?)null
            };
        }).ToList();

        return Ok(new { items = result, total, page, pageSize });
    }

    /// <summary>
    /// 清理过期的缓存项，避免内存泄漏
    /// </summary>
    private static void CleanExpiredCacheEntries()
    {
        var now = DateTime.UtcNow;
        var keysToRemove = _readCountCache
            .Where(kvp => (now - kvp.Value) > _cacheExpiration)
            .Select(kvp => kvp.Key)
            .ToList();

        foreach (var key in keysToRemove)
        {
            _readCountCache.TryRemove(key, out _);
        }
    }
}

